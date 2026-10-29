using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.DTO.Lesson;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class LessonAccessService : ILessonAccessService
    {
        private readonly UnitWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonAccessService(
            UnitWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }




        public async Task<(bool Success, string Message, object? Lesson)> GetLessonAsync(
            int lessonId,
            string studentId)
        {
            var lesson = _unitOfWork.Lessonrepo.GetById(lessonId);

            if (lesson == null)
            {
                return (false, "Lesson not found.", null);
            }

            // 1. Check whether the user has an active enrollment.
            var enrollment = string.IsNullOrWhiteSpace(studentId)
                ? null
                : _unitOfWork.TrackEnrollmentrepo
                    .GetByCondition(e =>
                        e.StudentId == studentId &&
                        e.TrackId == lesson.TrackId &&
                        e.Status == SessionStatus.Active)
                    .FirstOrDefault();

            bool hasFullAccess = enrollment != null;

            // 2. Check whether this lesson is unlocked for the student.
            if (hasFullAccess)
            {
                bool isUnlocked = _unitOfWork.LessonUnlockrepo
                    .GetByCondition(u =>
                        u.StudentId == studentId &&
                        u.LessonId == lessonId)
                    .Any();

                // The first lesson is accessible automatically.
                if (lesson.Order == _unitOfWork.Lessonrepo
                    .GetByCondition(l => l.TrackId == lesson.TrackId)
                    .Min(l => l.Order))
                {
                    isUnlocked = true;
                }

                if (!isUnlocked)
                {
                    return (
                        false,
                        "This lesson is locked. Pass its prerequisite quiz to unlock it.",
                        null
                    );
                }
            }

            // 3. Users without an active enrollment can access
            // free previews only.
            if (!hasFullAccess)
            {
                bool hasFreeContent = _unitOfWork.LessonContentrepo
                    .GetByCondition(c =>
                        c.LessonId == lessonId &&
                        c.IsFree == true)
                    .Any();

                if (!hasFreeContent)
                {
                    return (
                        false,
                        "This lesson has no free preview. Enroll to access it.",
                        null
                    );
                }
            }

            // 4. Return the lesson details.
            return (
                true,
                hasFullAccess
                    ? "Access granted."
                    : "Free preview access granted.",
                _mapper.Map<LessonResponseDTO>(lesson)
            );
        }



    }
}