using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using api.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;

namespace api.Utils
{
    /// <summary>
    /// Utility methods to manipulate images and upload them to Cloudinary
    /// </summary>
    public class ImageUtils
    {
        private readonly Cloudinary _cloudinary;

        /// <summary>
        /// ImageUtils constructor
        /// </summary>
        /// <param name="cloudinary"></param>
        public ImageUtils(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        /// <summary>
        /// Parses image format
        /// </summary>
        /// <param name="str"></param>
        /// <returns>ImageFormat</returns>
        public static ImageFormat ParseImageFormat(string str)
        {
            return (ImageFormat)typeof(ImageFormat)
                .GetProperty(str, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                ?.GetValue(null);
        }

        /// <summary>
        /// Resizes an image if it's wider than specified resolutin
        /// </summary>
        /// <param name="image"></param>
        /// <param name="fileName"></param>
        /// <param name="maxWidth"></param>
        /// <returns>Resized image Bitmap</returns>
        public static Image ResizeImage(Image image, int maxWidth)
        {
            decimal resizeRatio = (decimal)maxWidth / image.Width;
            var newHeight = image.Width <= maxWidth ? image.Height : Convert.ToInt32(image.Height * resizeRatio);
            var newWidth = image.Width <= maxWidth ? image.Width : Convert.ToInt32(image.Width * resizeRatio);
            var img = new Bitmap(image, newWidth, newHeight);

            return img;
        }

        /// <summary>
        /// Applies Cloudinary transformations and generates a link
        /// </summary>
        /// <param name="width"></param>
        /// <param name="quality"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="blurAmount">Blur amount added to the image (0 default)</param>
        /// <returns>A link to Cloudinary image</returns>
        public string GenerateCloudinaryLink(int width, int quality, string path, string fileName, int? blurAmount)
        {
            return _cloudinary.Api.UrlImgUp.Secure(true).Transform(
                new Transformation()
                    .Quality(quality)
                    .Width(width)
                    .Effect($"blur:{blurAmount ?? 0}")
                    .Crop("limit")
            ).BuildUrl(path + "/" + fileName);
        }

        /// <summary>
        /// Generates Cloudinary responsive image link (Suitable if Client app uses Cloudinary Library)
        /// </summary>
        /// <param name="cloudinaryFilePath"></param>
        /// <returns>Cloudinary Responsive URL</returns>
        public string GenerateResponsiveImage(string cloudinaryFilePath)
        {
            return _cloudinary.Api.UrlImgUp.Secure(true).Transform(
                new Transformation()
                    .Width("auto")
                    .Dpr("auto")
                    .Crop("scale")
                    .FetchFormat("auto")
            ).BuildUrl($"{cloudinaryFilePath}");
        }

        /// <summary>
        /// Generates an array of links with additional Cloudinary transformations applied (different sizes)
        /// </summary>
        /// <param name="resolutions"></param>
        /// <param name="quality"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns>A list of responsive Cloudinary links</returns>
        public List<ResponsiveUrl> GenerateUrlList(List<int> resolutions, int quality, string path, string fileName)
        {

            List<ResponsiveUrl> responsiveUrls = new();
            if (String.IsNullOrEmpty(fileName) || String.IsNullOrEmpty(path)) return responsiveUrls;
            foreach (int resolution in resolutions)
            {
                var link = GenerateCloudinaryLink(resolution, quality, path, fileName, 0);
                if (!String.IsNullOrWhiteSpace(link))
                {
                    responsiveUrls.Add(new ResponsiveUrl { Width = resolution, Url = link });
                }
            }

            return responsiveUrls;

        }

        /// <summary>
        /// Copies file to new memory stream, creates new Image from MS
        /// </summary>
        /// <param name="file"></param>
        /// <returns>MemoryStream, fileStream, Image</returns>
        public static async Task<(MemoryStream ms, Stream fileStream, Image image)> CopyImageToMs(IFormFile file)
        {
            var ms = new MemoryStream();
            var fileStream = file.OpenReadStream();
            await fileStream.CopyToAsync(ms);

            var image = Image.FromStream(fileStream);
            return (ms, fileStream, image);
        }


        public static (MemoryStream ms, Image image) CopyImageBytesToMs(byte[] imageBytes)
        {
            var ms = new MemoryStream(imageBytes);

            var image = Image.FromStream(ms);
            return (ms, image);
        }


        /// <summary>
        /// Converts MemoryStream to byte array
        /// </summary>
        /// <param name="ms"></param>
        /// <returns>Byte array</returns>
        public static async Task<byte[]> ConvertMsToBytes(MemoryStream ms)
        {
            byte[] value = ms.ToArray();
            await ms.DisposeAsync();
            return value;
        }

        public static string GenerateBase64String(string contentType, byte[] value) =>
            $"data:{contentType};base64,{Convert.ToBase64String(value)}";


        /// <summary>
        /// Encodes new image and saves it to memory stream
        /// </summary>
        /// <param name="newImage"></param>
        /// <param name="image"></param>
        /// <param name="ms"></param>
        /// <param name="imageFormat"></param>
        public static void EncodeBitmapToMs(Image newImage, Image image, MemoryStream ms, string imageFormat, long qualityParam = 50L)
        {
            var graphics = Graphics.FromImage(newImage);

            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImage(image, 0, 0, newImage.Width, newImage.Height);
            ms.SetLength(0);
            var qualityParamId = Encoder.Quality;
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(qualityParamId, qualityParam);
            var codec = ImageCodecInfo.GetImageDecoders()
                .FirstOrDefault(codec => codec.FormatID == ImageUtils.ParseImageFormat(imageFormat).Guid);


            if (codec != null) newImage.Save(ms, codec, encoderParameters);
        }

        public static async Task<string> GenerateBase64Placeholder(byte[] imageBytes, string contentType, int maxWidth, long quality)
        {
            var (ms, image) = ImageUtils.CopyImageBytesToMs(imageBytes);
            var newImage = ImageUtils.ResizeImage(image, maxWidth);
            var imageFormat = contentType.Replace("image/", "");

            ImageUtils.EncodeBitmapToMs(newImage, image, ms, imageFormat, quality);

            var value = await ImageUtils.ConvertMsToBytes(ms);

            return ImageUtils.GenerateBase64String(contentType, value);
        }
    }
}

