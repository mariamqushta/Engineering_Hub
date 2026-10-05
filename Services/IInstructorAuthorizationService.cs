namespace Engineering_Hub.Services
{
    public interface IInstructorAuthorizationService
    {
        bool CanManageTrack(
            int trackId,
            string instructorId);
    }
}