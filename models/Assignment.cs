using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Relationships

        public Lesson Lesson { get; set; }

        public ICollection<AssignmentSubmission> Submissions { get; set; }
    }
}
