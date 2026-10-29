using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class LessonQuiz
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        public string Title { get; set; } = string.Empty;

        public QuizType Type { get; set; }

        // Only 10 or 15 questions are allowed.
        public int QuestionCount { get; set; }

        // The required passing percentage.
        public decimal PassingScore { get; set; } = 80;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Lesson Lesson { get; set; } = null!;

        public ICollection<QuizQuestion> Questions { get; set; }
            = new List<QuizQuestion>();

        public ICollection<QuizAttempt> Attempts { get; set; }
            = new List<QuizAttempt>();
    }
}