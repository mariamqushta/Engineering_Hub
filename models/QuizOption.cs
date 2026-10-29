namespace Engineering_Hub.models
{
    public class QuizOption
    {
        public int Id { get; set; }

        public int QuizQuestionId { get; set; }

        public string Text { get; set; } = string.Empty;

        // Used by the backend when grading.
        // Never include this property in student-facing DTOs.
        public bool IsCorrect { get; set; }

        public QuizQuestion QuizQuestion { get; set; } = null!;
    }
}