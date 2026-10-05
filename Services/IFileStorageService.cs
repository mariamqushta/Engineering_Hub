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
    }
}