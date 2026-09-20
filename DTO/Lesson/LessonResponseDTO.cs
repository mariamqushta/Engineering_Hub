using Engineering_Hub.models;

namespace Engineering_Hub.DTO.Lesson
{
    public class LessonResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int Order { get; set; }
        public LessonType LessonType { get; set; }
        public string? ContentUrl { get; set; }
    }
}
