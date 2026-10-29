using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public int LessonQuizId { get; set; }

        public string StudentId { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedAt { get; set; }

        public int TotalQuestions { get; set; }

        public int CorrectAnswers { get; set; }

        public decimal Score { get; set; }

        public bool Passed { get; set; }

        public LessonQuiz LessonQuiz { get; set; } = null!;

        public ApplicationUser Student { get; set; } = null!;

        public ICollection<QuizAnswer> Answers { get; set; }
            = new List<QuizAnswer>();
    }
}