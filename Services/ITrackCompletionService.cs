using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ITrackCompletionService
    {
        Task<bool> CheckAndCompleteTrackAsync(
            int trackId,
            string studentId);
    }
}