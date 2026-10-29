using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ITrackCompletionService
    {
        Task<bool> CheckAndCompleteTrackAsync(
            int trackId,
            string studentId);

        Task<bool> IsTrackCompletedAsync(int trackId, string studentId);
    }
}