using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class Lesson
    {
        public int Id { get; set; }

        public int TrackId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }


        // Relationships

        public Track Track { get; set; }

        public ICollection<LessonContent> LessonContents { get; set; }
            = new List<LessonContent>();

        public ICollection<Assignment> Assignments { get; set; }
            = new List<Assignment>();

        public ICollection<LessonProgress> LessonProgresses { get; set; }
            = new List<LessonProgress>();
    }
}