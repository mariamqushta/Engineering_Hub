
using Engineering_Hub.DTOs.Quiz;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface IQuizService
    {
        Task<(bool Success, string Message, InstructorQuizDto? Data)>
            CreateQuizAsync(
                int lessonId,
                string instructorId,
                CreateLessonQuizDto dto);

        Task<List<InstructorQuizDto>>
            GetLessonQuizzesAsync(
                int lessonId,
                string instructorId);

        Task<StudentQuizDto?> GetStudentQuizAsync(
            int quizId,
            string studentId);

        Task<(bool Success, string Message, object? Data)>
            SubmitQuizAsync(
                int quizId,
                string studentId,
                SubmitQuizDto dto);

        Task<List<StudentQuizDto>>
            GetStudentLessonQuizzesAsync(
                int lessonId,
                string studentId);
    }
}
