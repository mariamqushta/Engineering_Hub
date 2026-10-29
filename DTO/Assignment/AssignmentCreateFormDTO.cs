using Microsoft.AspNetCore.Http;
using System;

namespace Engineering_Hub.DTOs
{
    public class AssignmentCreateFormDTO
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public IFormFile? Attachment { get; set; }
    }
}