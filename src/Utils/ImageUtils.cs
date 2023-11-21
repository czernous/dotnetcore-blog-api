using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using api.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

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
                new CloudinaryDotNet.Transformation()
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
        /// Converts byte[] to base64 string
        /// </summary>
        /// <param name="image"></param>
        /// <param name="format"></param>
        /// <returns>Base64 string</returns>
        public static string ConvertImageToBase64(byte[] imageBytes, string format)
        {
            // Get the base64 string
            var base64String = Convert.ToBase64String(imageBytes);

            return $"data:{format.ToString().ToLower()};base64,{base64String}";
        }

        public static async Task<string> GenerateBase64Placeholder(byte[] imageBytes, string contentType, int maxWidth)
        {
            byte[] resizedBytes = ImageUtils.ResizeBinaryImage(imageBytes, maxWidth);
            return ImageUtils.ConvertImageToBase64(resizedBytes, contentType);
        }

        public static byte[] ResizeBinaryImage(byte[] imageBytes, int maxWidth, SKFilterQuality quality = SKFilterQuality.Medium, SKEncodedImageFormat outputFormat = SKEncodedImageFormat.Webp, Int32 outputQuality = 65)
        {
            using MemoryStream ms = new MemoryStream(imageBytes);
            using SKBitmap sourceBitmap = SKBitmap.Decode(ms);
            float resizeRatio = (float)maxWidth / sourceBitmap.Width;
            int newHeight = sourceBitmap.Height <= maxWidth ? sourceBitmap.Height : (int)(sourceBitmap.Height * resizeRatio);
            int newWidth = sourceBitmap.Width <= maxWidth ? sourceBitmap.Width : (int)(sourceBitmap.Width * resizeRatio);

            using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(newWidth, newHeight), quality);
            using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
            using SKData data = scaledImage.Encode(outputFormat, outputQuality);

            return data.ToArray();
        }

        public static byte[] ConvertSKImageToByteArray(SKImage skImage)
        {
            using (var data = skImage.Encode())
            {
                return data.ToArray();
            }
        }
    }
}

