using Engineering_Hub.models;
using System;
using System.Collections.Generic;

namespace Engineering_Hub.DTOs.Quiz
{
    public class InstructorQuizDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuizType Type { get; set; }
        public int QuestionCount { get; set; }
        public decimal PassingScore { get; set; }
        public bool IsActive { get; set; }
        public List<InstructorQuizQuestionDto> Questions { get; set; }
            = new();
    }

    public class InstructorQuizQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<InstructorQuizOptionDto> Options { get; set; }
            = new();
    }

    public class InstructorQuizOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}