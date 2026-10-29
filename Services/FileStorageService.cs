using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty.");
            }

            string uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                folderName);

            Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(file.FileName);

            string storedFileName = $"{Guid.NewGuid()}{extension}";

            string filePath = Path.Combine(
                uploadsFolder,
                storedFileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return storedFileName;
        }

        public Task DeleteFileAsync(
            string storedFileName,
            string folderName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
            {
                return Task.CompletedTask;
            }

            string filePath = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                folderName,
                storedFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
        public async Task<string> SavePrivateFileAsync(
    IFormFile file,
    string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty.");
            }

            string privateUploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                folderName);

            Directory.CreateDirectory(privateUploadsFolder);

            string extension = Path.GetExtension(file.FileName);
            string storedFileName = $"{Guid.NewGuid()}{extension}";

            string filePath = Path.Combine(
                privateUploadsFolder,
                storedFileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            return storedFileName;
        }

        public Task DeletePrivateFileAsync(
            string storedFileName,
            string folderName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
            {
                return Task.CompletedTask;
            }

            string privateUploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                folderName);

            string filePath = Path.Combine(
                privateUploadsFolder,
                storedFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public string? GetPrivateFilePath(
    string storedFileName,
    string folderName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
            {
                return null;
            }

            // Only allow a filename, not a path supplied by a caller.
            string safeFileName = Path.GetFileName(storedFileName);

            if (safeFileName != storedFileName)
            {
                return null;
            }

            // Folder names are fixed by the application.
            if (folderName != "assignments" &&
                folderName != "submissions")
            {
                return null;
            }

            string filePath = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                folderName,
                safeFileName);

            return File.Exists(filePath) ? filePath : null;
        }
    }
}