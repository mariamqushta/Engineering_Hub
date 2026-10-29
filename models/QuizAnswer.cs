namespace Engineering_Hub.models
{
    public class QuizAnswer
    {
        public int Id { get; set; }

        public int QuizAttemptId { get; set; }

        public int QuizQuestionId { get; set; }

        public int SelectedOptionId { get; set; }

        public bool IsCorrect { get; set; }

        public QuizAttempt QuizAttempt { get; set; } = null!;

        public QuizQuestion QuizQuestion { get; set; } = null!;

        public QuizOption SelectedOption { get; set; } = null!;
    }
}