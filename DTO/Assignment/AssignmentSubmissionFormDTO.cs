using Microsoft.AspNetCore.Http;

namespace Engineering_Hub.DTOs
{
    public class AssignmentSubmissionFormDTO
    {
        public string? Content { get; set; }

        public IFormFile? File { get; set; }
    }
}