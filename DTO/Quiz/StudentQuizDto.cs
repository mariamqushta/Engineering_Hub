using Engineering_Hub.models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Engineering_Hub.DTOs.Quiz
{
    public class StudentQuizDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuizType Type { get; set; }
        public int QuestionCount { get; set; }
        public decimal PassingScore { get; set; }
        public List<StudentQuizQuestionDto> Questions { get; set; }
            = new();
    }

    public class StudentQuizQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<StudentQuizOptionDto> Options { get; set; }
            = new();
    }

    public class StudentQuizOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class SubmitQuizDto
    {
        [Required]
        [MinLength(1)]
        public List<SubmitQuizAnswerDto> Answers { get; set; }
            = new();
    }

    public class SubmitQuizAnswerDto
    {
        [Range(1, int.MaxValue)]
        public int QuestionId { get; set; }

        [Range(1, int.MaxValue)]
        public int SelectedOptionId { get; set; }
    }
}