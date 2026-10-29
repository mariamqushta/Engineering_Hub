using Engineering_Hub.models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Engineering_Hub.DTOs.Quiz
{
    public class CreateLessonQuizDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public QuizType Type { get; set; }

        [Range(10, 15)]
        public int QuestionCount { get; set; }

        public List<CreateQuizQuestionDto> Questions { get; set; }
            = new();
    }

    public class CreateQuizQuestionDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public List<CreateQuizOptionDto> Options { get; set; }
            = new();
    }

    public class CreateQuizOptionDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}