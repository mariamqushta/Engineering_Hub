namespace Engineering_Hub.models
{
    public class LessonContent
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        public int LessonTypeId { get; set; }

        // Used for Video, PDF, PowerPoint
        public string? ContentUrl { get; set; }

        // Original name uploaded by the instructor
        public string? OriginalFileName { get; set; }

        // Unique name used when saving the file on the server
        public string? StoredFileName { get; set; }

        // Example: video/mp4, application/pdf
        public string? ContentType { get; set; }

        // File size in bytes
        public long? FileSize { get; set; }

        // Used for Video, PDF, PowerPoint
        // null for Workshop and Interactive
        public bool? IsFree { get; set; }

        // Used when the content is a Workshop
        public int? WorkshopId { get; set; }

        // Used when the content is an Interactive Activity
        public int? InteractiveActivityId { get; set; }


        // Relationships

        public Lesson Lesson { get; set; }

        public LessonType LessonType { get; set; }

        public Workshop? Workshop { get; set; }

        public InteractiveActivity? InteractiveActivity { get; set; }
    }
}