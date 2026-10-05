using Microsoft.AspNetCore.Http;

namespace Engineering_Hub.Services
{
    public interface IFileValidationService
    {
        void ValidateFile(
            IFormFile file,
            string contentType);
    }
}