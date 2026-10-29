using System;

namespace Engineering_Hub.models
{
    public class LessonUnlock
    {
        public int Id { get; set; }

        public string StudentId { get; set; } = string.Empty;

        public int LessonId { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser Student { get; set; } = null!;

        public Lesson Lesson { get; set; } = null!;
    }
}