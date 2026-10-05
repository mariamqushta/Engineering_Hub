
namespace Engineering_Hub.DTO.LessonContent
{
    public class LessonContentResponseDTO
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        public int LessonTypeId { get; set; }

        public string LessonTypeName { get; set; }

        public string? ContentUrl { get; set; }

        public string? OriginalFileName { get; set; }

        public string? ContentType { get; set; }

        public long? FileSize { get; set; }

        public bool? IsFree { get; set; }

        public int? WorkshopId { get; set; }

        public int? InteractiveActivityId { get; set; }
    }
}

