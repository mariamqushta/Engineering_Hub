using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engineering_Hub.Services
{
    public class FileValidationService : IFileValidationService
    {
        private readonly Dictionary<string, string[]> _allowedExtensions =
            new Dictionary<string, string[]>
            {
                { "Video", new[] { ".mp4", ".webm" } },
                { "PDF", new[] { ".pdf" } },
                { "PowerPoint", new[] { ".ppt", ".pptx" } }
            };

        private readonly Dictionary<string, long> _maxFileSizes =
            new Dictionary<string, long>
            {
                { "Video", 500 * 1024 * 1024 },       // 500 MB
                { "PDF", 20 * 1024 * 1024 },          // 20 MB
                { "PowerPoint", 50 * 1024 * 1024 }    // 50 MB
            };

        public void ValidateFile(
            IFormFile file,
            string contentType)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty.");
            }

            if (!_allowedExtensions.ContainsKey(contentType))
            {
                throw new ArgumentException(
                    $"File uploads are not supported for content type '{contentType}'.");
            }

            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions[contentType]
                .Contains(extension))
            {
                throw new ArgumentException(
                    $"Invalid file extension '{extension}' for {contentType}.");
            }

            if (file.Length > _maxFileSizes[contentType])
            {
                throw new ArgumentException(
                    $"File size exceeds the maximum allowed size for {contentType}.");
            }
        }
    }
}