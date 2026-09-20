using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ILessonCompletionService
    {
        Task<(bool Success, string Message)> CompleteLessonAsync(
            int lessonId,
            string studentId);
    }
}
