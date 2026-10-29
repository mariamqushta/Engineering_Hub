using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(
            IFormFile file,
            string folderName);

        Task DeleteFileAsync(
            string storedFileName,
            string folderName);

        Task<string> SavePrivateFileAsync(
    IFormFile file,
    string folderName);

        Task DeletePrivateFileAsync(
            string storedFileName,
            string folderName);

        string? GetPrivateFilePath(
    string storedFileName,
    string folderName);
    }
}