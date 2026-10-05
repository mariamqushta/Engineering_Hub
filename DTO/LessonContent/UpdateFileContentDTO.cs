using Microsoft.AspNetCore.Http;
namespace Engineering_Hub.DTO.LessonContent 
{ 
    public class UpdateFileContentDTO 
    { 
        public IFormFile? File { get; set; } 
        public bool? IsFree { get; set; } 
    } 
}
