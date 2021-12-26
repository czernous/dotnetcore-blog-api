using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using api.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json.Linq;
using api.Models;
using api.Services;
using api.Utils;

#pragma warning disable 1591

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiKeyAuth]

    public class ImagesController : ControllerBase
    {

        private readonly Cloudinary _cloudinary;
        private readonly ImageService _imageService;
        private readonly PostService _postService;
        private readonly ImageUtils _imageUtils;

        /// Images controller
        public ImagesController(Cloudinary cloudinary, ImageService imageService, PostService postService)
        {
            _imageUtils = new ImageUtils(cloudinary);
            _imageService = imageService;
            _postService = postService;
            _cloudinary = cloudinary;
        }

        /*
        private static Image ResizeImage(Image image, string fileName, int maxWidth)
        {
            return ImageUtils.ResizeImage(image, fileName, maxWidth);
        }
*/


        // private string GenerateResponsiveLink(int width, int quality, string path, string fileName)
        // {
        //     return _imageUtils.GenerateResponsiveLink(width, quality, path, fileName);
        // }
        //
        // private List<string> GenerateUrlList(List<int> resolutions, int quality, string path, string fileName)
        // {
        //     return _imageUtils.GenerateUrlList(resolutions, quality, path, fileName);
        // }
        [HttpGet]
        public async Task<ActionResult<List<CldImage>>> Get() =>
            await _imageService.GetAllAsync();

        [HttpGet("{id:length(24)}", Name = "GetImage")]
        public ActionResult<CldImage> Get(string id)
        {
            var image = _imageService.GetById(id);

            if (image == null) return NotFound();

            return image;
        }


        /// <summary>
        /// Uploads image to cloudinary and save it's metadata(urls) to the database
        /// </summary>
        /// <returns>Void</returns>
        /// <remarks>
        /// The end point accepts "filename" and "folder" query string params
        /// 
        /// The process:
        ///
        ///   1. The code checks if the uploaded image already exists in the database(to be implemented)
        ///   2. The uploaded file is resized if larger than 2400px wide
        ///   3. The file is converted to base64 string
        ///   4. The code checks if the query string contains "folder" or "filename" params
        ///   5. If the query string is empty, the file is uploaded to Cloudinary with a random name to the root folder \n
        ///   7. Otherwise it is uploaded to the specified folder with specified filename
        ///   8. A list of URLs with applied transformations(resized) is created
        ///   9. A new CldImage entity is created and saved to the database
        ///
        /// </remarks>
        /// <response code="200">If the image was uploaded</response>
        /// <response code="415">If the uploaded file content-type is incorrect</response>
        [HttpPost]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {

            if (file.ContentType.ToLower() != "image/jpeg" &&
                file.ContentType.ToLower() != "image/jpg" &&
                file.ContentType.ToLower() != "image/png")
            {
                // not a .jpg or .png file
                return StatusCode(415);

            }

            // read asset(file) name and folder from query string
            var cloudinaryFileName = Request.Query["filename"];
            var cloudinaryStorageFolder = Request.Query["folder"];

            // check if image exists in the DB
            var foundImage = await _imageService.GetByNameAsync(cloudinaryFileName);
            if (foundImage != null) return BadRequest($"The image with filename '{cloudinaryFileName}' already exists.\nPlease Choose a different name.");


            var extension = "." + file.FileName.Split('.')[^1];
            var fileName = Path.GetFileName(file.FileName);

            // Convert Image to Base64 Image string
            var (ms, fileStream, image) = await ImageUtils.CopyImageToMs(file);


            var newImage = ImageUtils.ResizeImage(image, fileName, 2400);
            var imageFormat = file.ContentType.Replace("image/", "");

            ImageUtils.EncodeBitmapToMs(newImage, image, ms, imageFormat);

            var value = await ImageUtils.ConvertMsToBytes(ms);

            var b64ImageString = $"data:{file.ContentType};base64,{Convert.ToBase64String(value)}";

            // SAVE TO CLOUDINARY
            var results = new List<Dictionary<string, string>>();
            if (results == null) throw new ArgumentNullException(nameof(results));
            var imageProperties = new Dictionary<string, string>();


            var cloudinaryUploadParams = new ImageUploadParams()
            {
                File = new FileDescription(@$"{b64ImageString}"),
                PublicId = $"{cloudinaryStorageFolder}/{cloudinaryFileName}",
            };

            // if no folder and filename specified upload to root folder with random name(duplicates possible)

            if (String.IsNullOrEmpty(cloudinaryFileName) || String.IsNullOrEmpty(cloudinaryStorageFolder)) cloudinaryUploadParams.PublicId = null;

            //  uploead image to cloudinary
            var result = await _cloudinary.UploadAsync(cloudinaryUploadParams).ConfigureAwait(false);

            foreach (var token in result.JsonObj.Children())
            {
                if (token is JProperty prop)
                {
                    imageProperties.Add(prop.Name, prop.Value.ToString());
                }
            }

            // create a list of resized image links
            var urlList = _imageUtils.GenerateUrlList(new List<int>() { 512, 718, 1024, 1280 }, 70, cloudinaryStorageFolder, cloudinaryFileName);

            // create new image to save to DB
            CldImage imageData = new CldImage
            {
                Bytes = (int)result.Bytes,
                Created = DateTime.Now,
                Format = result.Format,
                Height = result.Height,
                Path = result.Url.AbsolutePath,
                PublicId = result.PublicId,
                Name = result.PublicId.Substring(result.PublicId.LastIndexOf('/') + 1),
                ResourceType = result.ResourceType,
                SecureUrl = result.SecureUrl.AbsoluteUri,
                Signature = result.Signature,
                Type = result.JsonObj["type"]?.ToString(),
                Url = result.Url.AbsoluteUri,
                UsedInPost = null,
                ResponsiveUrls = urlList,
                Version = int.Parse(result.Version),
                Width = result.Width
            };

            results.Add(imageProperties);

            if (ModelState.IsValid)
            {

                await _imageService.CreateAsync(imageData);

                return CreatedAtRoute("GetCategory", new { id = imageData.Id.ToString() }, image);
            }
            else
            {
                return BadRequest(ModelState);
            }

        }

        [HttpDelete("{id:Length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var image = _imageService.GetById(id);

            if (image == null) return NotFound();

            var post = await _postService.GetOneByImage(image);

            if (post != null)
            {
                post.ImageUrl = null;
                post.ResponsiveImgs = null;

                Post updatedPost = post as Post;

                await _postService.UpdateAsync(post.Id, updatedPost);
            }

            await _imageService.RemoveAsync(image.Id);

            var delResParams = new DelResParams()
            {
                PublicIds = new List<string> { image.PublicId }
            };

            await _cloudinary.DeleteResourcesAsync(delResParams);

            return NoContent();
        }
    }
}