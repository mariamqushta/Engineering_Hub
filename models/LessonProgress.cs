using System;

namespace Engineering_Hub.models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public int LessonId { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }


        // Relationships

        public ApplicationUser Student { get; set; }

        public Lesson Lesson { get; set; }
    }
}
