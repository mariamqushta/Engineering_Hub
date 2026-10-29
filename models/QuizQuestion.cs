using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        public int LessonQuizId { get; set; }

        public string Text { get; set; } = string.Empty;

        public int Order { get; set; }

        public LessonQuiz LessonQuiz { get; set; } = null!;

        public ICollection<QuizOption> Options { get; set; }
            = new List<QuizOption>();
    }
}