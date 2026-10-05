using Engineering_Hub.UnitOfWork;
using System.Linq;

namespace Engineering_Hub.Services
{
    public class InstructorAuthorizationService
        : IInstructorAuthorizationService
    {
        private readonly UnitWork _unitOfWork;

        public InstructorAuthorizationService(
            UnitWork unitWork)
        {
            _unitOfWork = unitWork;
        }

        public bool CanManageTrack(
            int trackId,
            string instructorId)
        {
            var track = _unitOfWork.Trackrepo
                .GetById(trackId);

            if (track == null || !track.IsActive)
            {
                return false;
            }

            var instructor = _unitOfWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == trackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            return instructor != null;
        }
    }
}