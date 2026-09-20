using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ILessonAccessService
    {
        Task<(bool Success, string Message, object? Lesson)> GetLessonAsync(
          int lessonId,
          string studentId);
    }
}
