using Microsoft.AspNetCore.Http;

namespace Engineering_Hub.DTO.LessonContent
{
    public class AddFileContentDTO
    {
        public int LessonId { get; set; }

        public int LessonTypeId { get; set; }

        public IFormFile File { get; set; }

        public bool IsFree { get; set; }
    }
}