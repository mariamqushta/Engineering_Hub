using Engineering_Hub.models;

namespace Engineering_Hub.DTO
{
    public class LessonDTO
    {
        public int TrackId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int Order { get; set; }
        public LessonType LessonType { get; set; }
        public string? ContentUrl { get; set; }
    }
}