
using Engineering_Hub.DTOs.Quiz;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class QuizService : IQuizService
    {
        private readonly UnitWork _unitOfWork;
        private readonly InstructorAuthorizationService _instructorAuthorization;
        private readonly ILessonCompletionService _lessonCompletionService;
        private readonly ITrackCompletionService _trackCompletionService;
        public QuizService(
     UnitWork unitOfWork,
     InstructorAuthorizationService instructorAuthorization,
     ILessonCompletionService lessonCompletionService,
     ITrackCompletionService trackCompletionService)
        {
            _unitOfWork = unitOfWork;
            _instructorAuthorization = instructorAuthorization;
            _lessonCompletionService = lessonCompletionService;
            _trackCompletionService = trackCompletionService;
        }

        public async Task<(bool Success, string Message, InstructorQuizDto? Data)>
            CreateQuizAsync(
                int lessonId,
                string instructorId,
                CreateLessonQuizDto dto)
        {
            var lesson = _unitOfWork.Lessonrepo.GetById(lessonId);

            if (lesson == null)
                return (false, "Lesson not found.", null);

            if (!_instructorAuthorization.CanManageTrack(
                    lesson.TrackId, instructorId))
            {
                return (false,
                    "You are not authorized to manage this lesson.", null);
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
                return (false, "Quiz title is required.", null);

            if (dto.Type != QuizType.EndOfLesson &&
                dto.Type != QuizType.Prerequisite)
            {
                return (false, "Invalid quiz type.", null);
            }

            if (dto.QuestionCount != 10 && dto.QuestionCount != 15)
                return (false, "A quiz must contain 10 or 15 questions.", null);

            if (dto.Questions == null ||
                dto.Questions.Count != dto.QuestionCount)
            {
                return (false,
                    "The number of submitted questions must match QuestionCount.",
                    null);
            }

            foreach (var question in dto.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                    return (false, "Every question must have text.", null);

                if (question.Options == null || question.Options.Count < 2)
                    return (false,
                        "Every question must have at least two options.", null);

                if (question.Options.Any(o =>
                        string.IsNullOrWhiteSpace(o.Text)))
                {
                    return (false, "Every option must have text.", null);
                }

                if (question.Options.Count(o => o.IsCorrect) != 1)
                    return (false,
                        "Every question must have exactly one correct option.",
                        null);
            }

            // Allow only one quiz of each type per lesson.
            var existingQuizzes = _unitOfWork.LessonQuizrepo
                .GetByCondition(q =>
                    q.LessonId == lessonId &&
                    q.Type == dto.Type);

            if (existingQuizzes.Any())
            {
                return (false,
                    "A quiz of this type already exists for this lesson.",
                    null);
            }

            var quiz = new LessonQuiz
            {
                LessonId = lessonId,
                Title = dto.Title.Trim(),
                Type = dto.Type,
                QuestionCount = dto.QuestionCount,
                PassingScore = 80,
                IsActive = true,
                Questions = dto.Questions.Select((q, index) =>
                    new QuizQuestion
                    {
                        Text = q.Text.Trim(),
                        Order = index + 1,
                        Options = q.Options.Select(o =>
                            new QuizOption
                            {
                                Text = o.Text.Trim(),
                                IsCorrect = o.IsCorrect
                            }).ToList()
                    }).ToList()
            };

            _unitOfWork.LessonQuizrepo.add(quiz);
            await _unitOfWork.SaveAsync();

            return (true, "Quiz created successfully.",
                MapInstructorQuiz(quiz));
        }

        public Task<List<InstructorQuizDto>> GetLessonQuizzesAsync(
            int lessonId,
            string instructorId)
        {
            var lesson = _unitOfWork.Lessonrepo.GetById(lessonId);

            if (lesson == null ||
                !_instructorAuthorization.CanManageTrack(
                    lesson.TrackId, instructorId))
            {
                return Task.FromResult(new List<InstructorQuizDto>());
            }

            var quizzes = _unitOfWork.LessonQuizrepo
                .GetByConditionWithInclude(
                    q => q.LessonId == lessonId,
                    q => q.Questions);

            var result = quizzes.Select(MapInstructorQuiz).ToList();

            return Task.FromResult(result);
        }

        private static InstructorQuizDto MapInstructorQuiz(LessonQuiz quiz)
        {
            return new InstructorQuizDto
            {
                Id = quiz.Id,
                LessonId = quiz.LessonId,
                Title = quiz.Title,
                Type = quiz.Type,
                QuestionCount = quiz.QuestionCount,
                PassingScore = quiz.PassingScore,
                IsActive = quiz.IsActive,
                Questions = quiz.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new InstructorQuizQuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Order = q.Order,
                        Options = q.Options.Select(o =>
                            new InstructorQuizOptionDto
                            {
                                Id = o.Id,
                                Text = o.Text,
                                IsCorrect = o.IsCorrect
                            }).ToList()
                    }).ToList()
            };
        }
        public Task<StudentQuizDto?> GetStudentQuizAsync(
    int quizId,
    string studentId)
        {
            var quiz = _unitOfWork.LessonQuizrepo
                .GetById(quizId);

            if (quiz == null || !quiz.IsActive)
                return Task.FromResult<StudentQuizDto?>(null);

            var lesson = _unitOfWork.Lessonrepo
                .GetById(quiz.LessonId);

            if (lesson == null)
                return Task.FromResult<StudentQuizDto?>(null);

            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == lesson.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            if (enrollment == null)
                return Task.FromResult<StudentQuizDto?>(null);
            // Only EndOfLesson quizzes require the lesson to be unlocked.
            if (quiz.Type == QuizType.EndOfLesson)
            {
                var isUnlocked = _unitOfWork.LessonUnlockrepo
                    .GetByCondition(u =>
                        u.StudentId == studentId &&
                        u.LessonId == lesson.Id)
                    .Any();

                var firstLesson = _unitOfWork.Lessonrepo
                    .GetByCondition(l => l.TrackId == lesson.TrackId)
                    .OrderBy(l => l.Order)
                    .FirstOrDefault();

                if (firstLesson?.Id == lesson.Id)
                {
                    isUnlocked = true;
                }

                if (!isUnlocked)
                {
                    return Task.FromResult<StudentQuizDto?>(null);
                }
            }
            var questions = _unitOfWork.QuizQuestionrepo
                .GetByCondition(q => q.LessonQuizId == quizId)
                .OrderBy(q => q.Order)
                .ToList();

            var result = new StudentQuizDto
            {
                Id = quiz.Id,
                LessonId = quiz.LessonId,
                Title = quiz.Title,
                Type = quiz.Type,
                QuestionCount = quiz.QuestionCount,
                PassingScore = quiz.PassingScore,
                Questions = questions.Select(q =>
                    new StudentQuizQuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Order = q.Order,
                        Options = _unitOfWork.QuizOptionrepo
                            .GetByCondition(o =>
                                o.QuizQuestionId == q.Id)
                            .Select(o => new StudentQuizOptionDto
                            {
                                Id = o.Id,
                                Text = o.Text
                            })
                            .ToList()
                    }).ToList()
            };

            return Task.FromResult<StudentQuizDto?>(result);
        }

        public async Task<(bool Success, string Message, object? Data)> SubmitQuizAsync(
      int quizId,
      string studentId,
      SubmitQuizDto dto)
        {
            if (dto == null || dto.Answers == null)
                return (false, "Quiz answers are required.", null);

            var answers = dto.Answers;
            var quiz = _unitOfWork.LessonQuizrepo
                .GetByCondition(q => q.Id == quizId && q.IsActive)
                .FirstOrDefault();

            if (quiz == null)
                return (false, "Quiz not found.", null);

            var lesson = _unitOfWork.Lessonrepo
                .GetByCondition(l => l.Id == quiz.LessonId)
                .FirstOrDefault();

            if (lesson == null)
                return (false, "Lesson not found.", null);

            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == lesson.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            if (enrollment == null)
                return (false, "You are not enrolled in this track.", null);

            // Check whether the lesson is unlocked for this student.
            // Keep your existing lesson-unlocking logic here if it differs.

            var questions = _unitOfWork.QuizQuestionrepo
                .GetByCondition(q => q.LessonQuizId == quizId)
                .ToList();

            if (!questions.Any())
                return (false, "This quiz has no questions.", null);

            if (answers == null || answers.Count != questions.Count)
                return (false, "Please answer all quiz questions.", null);

            int correctAnswers = 0;

            foreach (var answer in answers)
            {
                var question = questions
                    .FirstOrDefault(q => q.Id == answer.QuestionId);

                if (question == null)
                    return (false, "One or more submitted questions are invalid.", null);

                var selectedOption = _unitOfWork.QuizOptionrepo
                    .GetByCondition(o =>
                        o.Id == answer.SelectedOptionId &&
                        o.QuizQuestionId == question.Id)
                    .FirstOrDefault();

                if (selectedOption == null)
                    return (false, "One or more selected answers are invalid.", null);

                if (selectedOption.IsCorrect)
                    correctAnswers++;
            }

            decimal score = questions.Count == 0
                ? 0
                : (decimal)correctAnswers / questions.Count * 100;

            bool passed = score >= quiz.PassingScore;

            var attempt = new QuizAttempt
            {
                LessonQuizId = quizId,
                StudentId = studentId,
                TotalQuestions = questions.Count,
                CorrectAnswers = correctAnswers,
                Score = score,
                Passed = passed,
                SubmittedAt = DateTime.UtcNow
            };

            _unitOfWork.QuizAttemptrepo.add(attempt);

            // Preserve your existing prerequisite-quiz progression logic here.
            // A passed prerequisite quiz should unlock the appropriate next lesson.

            await _unitOfWork.SaveAsync();

            bool lessonCompleted = false;
            bool trackCompleted = false;
            string lessonCompletionMessage = "";

            if (passed && quiz.Type == QuizType.EndOfLesson)
            {
                var result = await _lessonCompletionService.CompleteLessonAsync(
                    lesson.Id,
                    studentId);

                lessonCompleted = result.Success;
                lessonCompletionMessage = result.Message;

                if (lessonCompleted)
                {
                    trackCompleted = await _trackCompletionService
                        .IsTrackCompletedAsync(lesson.TrackId, studentId);
                }
            }

            return (
                true,
                "Quiz submitted successfully.",
                new
                {
                    attempt.Id,
                    attempt.TotalQuestions,
                    attempt.CorrectAnswers,
                    attempt.Score,
                    attempt.Passed,
                    attempt.SubmittedAt,
                    lessonCompleted,
                    lessonCompletionMessage,
                    trackCompleted,
                    UnlockedNextLesson =
                        quiz.Type == QuizType.Prerequisite && attempt.Passed
                }
            );
        }

        public Task<List<StudentQuizDto>> GetStudentLessonQuizzesAsync(
    int lessonId,
    string studentId)
        {
            var lesson = _unitOfWork.Lessonrepo.GetById(lessonId);

            if (lesson == null)
            {
                return Task.FromResult(new List<StudentQuizDto>());
            }

            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == lesson.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            if (enrollment == null)
            {
                return Task.FromResult(new List<StudentQuizDto>());
            }

            var quizzes = _unitOfWork.LessonQuizrepo
                .GetByCondition(q =>
                    q.LessonId == lessonId &&
                    q.IsActive)
                .ToList();

            var result = new List<StudentQuizDto>();

            foreach (var quiz in quizzes)
            {
                // End-of-Lesson quizzes are listed only if the lesson is unlocked.
                if (quiz.Type == QuizType.EndOfLesson)
                {
                    var isUnlocked = _unitOfWork.LessonUnlockrepo
                        .GetByCondition(u =>
                            u.StudentId == studentId &&
                            u.LessonId == lessonId)
                        .Any();

                    var firstLesson = _unitOfWork.Lessonrepo
                        .GetByCondition(l => l.TrackId == lesson.TrackId)
                        .OrderBy(l => l.Order)
                        .FirstOrDefault();

                    if (firstLesson?.Id != lessonId && !isUnlocked)
                    {
                        continue;
                    }
                }

                var questions = _unitOfWork.QuizQuestionrepo
                    .GetByCondition(q => q.LessonQuizId == quiz.Id)
                    .OrderBy(q => q.Order)
                    .ToList();

                result.Add(new StudentQuizDto
                {
                    Id = quiz.Id,
                    LessonId = quiz.LessonId,
                    Title = quiz.Title,
                    Type = quiz.Type,
                    QuestionCount = quiz.QuestionCount,
                    PassingScore = quiz.PassingScore,
                    Questions = questions.Select(q =>
                        new StudentQuizQuestionDto
                        {
                            Id = q.Id,
                            Text = q.Text,
                            Order = q.Order,
                            Options = _unitOfWork.QuizOptionrepo
                                .GetByCondition(o =>
                                    o.QuizQuestionId == q.Id)
                                .Select(o => new StudentQuizOptionDto
                                {
                                    Id = o.Id,
                                    Text = o.Text
                                })
                                .ToList()
                        }).ToList()
                });
            }

            return Task.FromResult(result);
        }
    }
}
