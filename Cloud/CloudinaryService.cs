using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Cloud
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _cloudName;

        public CloudinaryService(IConfiguration config)
        {
            var settings = config.GetSection("CloudinarySettings").Get<CloudinarySettings>();

            if (settings == null || string.IsNullOrEmpty(settings.CloudName))
            {
                throw new ArgumentNullException("Cloudinary settings are missing or incomplete in configuration.");
            }

            _cloudName = settings.CloudName;
            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"{Guid.NewGuid()}",
                Transformation = new Transformation().Width(500).Crop("limit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl.AbsoluteUri;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var publicId = GetPublicIdFromUrl(imageUrl);

                if (string.IsNullOrEmpty(publicId)) return false;

                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                return deletionResult.Result == "ok";
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> UpdateImageAsync(string oldImageUrl, IFormFile newImageFile)
        {
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await DeleteImageAsync(oldImageUrl);
            }

            return await UploadImageAsync(newImageFile);
        }

        private string GetPublicIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (!url.Contains(_cloudName))
            {
                return null;
            }

            var startTag = "/upload/";
            var startIndex = url.IndexOf(startTag);

            if (startIndex == -1) return null;

            var segment = url.Substring(startIndex + startTag.Length);

            var versionTagRegex = new Regex(@"v\d+/");
            var match = versionTagRegex.Match(segment);

            if (match.Success)
            {
                var publicIdWithExtension = segment.Substring(match.Index + match.Length);

                var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    return publicIdWithExtension.Substring(0, lastDotIndex);
                }

                return publicIdWithExtension;
            }

            return null;
        }

        public async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files)
        {
            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                var url = await UploadImageAsync(file);
                uploadedUrls.Add(url);
            }

            return uploadedUrls;
        }

        public async Task<List<ImageUploadResult>> UploadMultipleImagesWithDetailsAsync(List<IFormFile> files)
        {
            var results = new List<ImageUploadResult>();

            foreach (var file in files)
            {
                using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = $"blog/{Guid.NewGuid()}",
                    Transformation = new Transformation()
                        .Width(1200)
                        .Crop("limit")
                        .Quality("auto:good")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                results.Add(result);
            }

            return results;
        }

        public async Task<List<bool>> DeleteMultipleImagesAsync(List<string> imageUrls)
        {
            var deletionTasks = imageUrls.Select(url => DeleteImageAsync(url));
            var results = await Task.WhenAll(deletionTasks);

            return results.ToList();
        }

    }
}