using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using api.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json.Linq;
using api.Models;
using api.Interfaces;
using api.Utils;
using MongoDB.Driver;
using Internal;
using Microsoft.Extensions.Logging;

#pragma warning disable 1591

namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiKeyAuth]

    public class ImagesController : ControllerBase
    {

        private readonly Cloudinary _cloudinary;
        private readonly IMongoRepository<CldImage> _imageRepository;
        private readonly IMongoRepository<Post> _postsRepository;
        private readonly ImageUtils _imageUtils;

        private readonly ILogger<ImagesController> _logger;

        /// Images controller
        public ImagesController(Cloudinary cloudinary, IMongoRepository<CldImage> imageRepository, IMongoRepository<Post> postsRepository, ILogger<ImagesController> logger)
        {
            _imageUtils = new ImageUtils(cloudinary);
            _imageRepository = imageRepository;
            _postsRepository = postsRepository;
            _cloudinary = cloudinary;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<CldImage> Get() => _imageRepository.FilterBy(Id => true);

        [HttpGet("{id:length(24)}", Name = "GetImage")]
        public ActionResult<CldImage> Get(string id)
        {
            _logger.LogInformation("Received an image get request.");
            var image = _imageRepository.FindById(id);

            if (image == null) return NotFound();

            return image;
        }


        /// <summary>
        /// Uploads image to cloudinary and save it's metadata(urls) to the database
        /// </summary>
        /// <returns>Void</returns>

        /// <remarks>
        /// The end point accepts "filename" and "folder" query string params as well as "max-width", "widths" and "q" (quality)
        /// 
        /// If the "widths" and "q" params are not passed, they default to:
        /// 
        /// max-width=2400 - accepts ints between 0 and 9999
        /// 
        /// widths=512,718,1024,1280 - note the param only accepts comma separated ints
        /// 
        /// q=70 - accepts ints between 0 and 100
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
        /// example qs object:  http://fs-app.loc/backend/images?filename=test-image-3/<![CDATA[&]]>folder=test-api-folder/test-images/<![CDATA[&]]>max-width=2400<![CDATA[&]]>widths=512,768,1140,1920<![CDATA[&]]>q=70
        ///
        /// </remarks>
        /// <response code="200">If the image was uploaded</response>
        /// <response code="415">If the uploaded file content-type is incorrect or the request body is not multipart form (file)</response>
        [HttpPost(Name = "UploadImage")]
        public async Task<ActionResult> UploadFile(string filename, string folder, string? widths, string? maxWidth, string? quality)
        {


            string cT = Request.Headers.ContentType;
            _logger.LogInformation("Received an image upload request.");
            _logger.LogInformation($"With content-type:  {cT}");


            if (!Request.Headers.ContainsKey("Content-Type"))
            {
                _logger.LogWarning("Content-Type header is missing");
                return BadRequest("Content-Type header is missing");
            }

            using (var reqMs = new MemoryStream())
            {
                await Request.Body.CopyToAsync(reqMs);
                var binaryImage = reqMs.ToArray();

                _logger.LogInformation($"Received image data: {binaryImage.Length} bytes");

                // comma separated ints regex = ^[0-9]{1,4},?([0-9]{1,4},?)*$

                string widthsPattern = @"^[0-9]{1,4},?([0-9]{1,4},?)*$";
                string qualityPattern = @"[0-9]{1,3}$";
                string maxWidthPattern = @"[0-9]{1,4}$";

                bool isCommaSeparatedInts = widths != null && Regex.IsMatch(widths, widthsPattern);
                bool isQualityInt = quality != null && Regex.IsMatch(quality, qualityPattern);
                bool isMaxWidthInt = maxWidth != null && Regex.IsMatch(maxWidth, maxWidthPattern);

                int qualityInt = quality != null && isQualityInt ? Int16.Parse(quality) : 70;
                int maxWidthInt = maxWidth != null && isMaxWidthInt ? Int16.Parse(maxWidth) : 2400;

                if (widths != null && !isCommaSeparatedInts) return BadRequest("Widths should be a list of comma separated ints");
                if (quality != null && !isQualityInt) return BadRequest("Quality (q) should be an int between 0 and 100");

                List<string> widthsList = widths != null ? widths?.Split(',')?.ToList() : null;
                List<int> widthsListInt = widthsList != null ? widthsList.Select(int.Parse).ToList() : new List<int>() { 512, 718, 1024, 1280 }; // use fallback values if null

                var validContentTypes = new List<string> { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/avif" };
                string contentType = "image/jpeg"; // Set a default content type

                string contentTypeHeader = Request.Headers["Content-Type"];


                if (!string.IsNullOrWhiteSpace(contentTypeHeader) && validContentTypes.Contains(contentTypeHeader.ToLower()))
                {
                    contentType = contentTypeHeader.ToLower();
                }
                else
                {
                    _logger.LogWarning($"Unsupported content type received:  {contentType}");
                    return StatusCode(415); // Unsupported Media Type
                }

                if (string.IsNullOrWhiteSpace(filename)) return BadRequest("Please pass Cloudinary filename in the query string");
                if (string.IsNullOrWhiteSpace(folder)) return BadRequest("Please pass Cloudinary folder name / path in the query string");
                if (maxWidth != null && !isMaxWidthInt) return BadRequest("maxWidth should be an int between 0 and 9999");

                var imageFilter = Builders<CldImage>.Filter.Eq("Name", filename);

                // check if image exists in the DB
                var foundImage = _imageRepository.FindOne(imageFilter);
                if (foundImage != null) return BadRequest($"The image with filename '{filename}' already exists.\nPlease Choose a different name.");

                // var extension = "." + file.FileName.Split('.')[^1];

                var newImage = ImageUtils.ResizeBinaryImage(binaryImage, maxWidthInt);

                string b64ImageString = ImageUtils.ConvertImageToBase64(newImage, "image/webp"); // resized image is hardcoded as webp by default

                // SAVE TO CLOUDINARY
                var results = new List<Dictionary<string, string>>();
                if (results == null) throw new ArgumentNullException(nameof(results));
                var imageProperties = new Dictionary<string, string>();


                var cloudinaryUploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(@$"{b64ImageString}"),
                    PublicId = $"{folder}/{filename}",
                };

                // if no folder and filename specified upload to root folder with random name(duplicates possible)

                if (String.IsNullOrEmpty(filename) || String.IsNullOrEmpty(folder)) cloudinaryUploadParams.PublicId = null;

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
                var urlList = _imageUtils.GenerateUrlList(widthsListInt, qualityInt, folder, filename);

                // create new image to save to DB
                CldImage imageData = new CldImage
                {
                    Bytes = (int)result.Bytes,
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
                    ThumbnailUrl = _imageUtils.GenerateCloudinaryLink(250, 70, folder, filename, 0),
                    BlurredImageUrl = await ImageUtils.GenerateBase64Placeholder(newImage, "image/webp", 100),
                    Version = int.Parse(result.Version),
                    Width = result.Width
                };

                results.Add(imageProperties);

                if (ModelState.IsValid)
                {

                    await _imageRepository.InsertOneAsync(imageData);

                    string imageId = imageData.Id.ToString();
                    _logger.LogInformation("Image data saved to the database.");
                    return CreatedAtRoute("UploadImage", new { id = imageId }, imageData);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }

        }

        [HttpDelete("{id:Length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var image = _imageRepository.FindById(id);

            if (image == null) return NotFound();

            var postsFilter = Builders<Post>.Filter.Eq("ImageUrl", image.SecureUrl);

            var post = _postsRepository.FindOne(postsFilter);

            if (post != null)
            {
                post.ImageUrl = null;

                post.ResponsiveImgs = null;
                post.ImageAltText = null;
                post.BlurredImageUrl = null;

                post.Meta.OpenGraph.ImageUrl =
                    post.Meta.OpenGraph.ImageUrl == image.SecureUrl
                        ? null
                        : post.Meta.OpenGraph.ImageUrl;

                Post updatedPost = post as Post;

                await _postsRepository.ReplaceOneAsync(updatedPost);
            }

            await _imageRepository.DeleteByIdAsync(image.Id.ToString());

            var delResParams = new DelResParams()
            {
                PublicIds = new List<string> { image.PublicId }
            };

            await _cloudinary.DeleteResourcesAsync(delResParams);

            return NoContent();
        }
    }
}