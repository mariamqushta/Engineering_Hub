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
            // 1. Find the requested lesson
            var lesson = _unitOfWork.Lessonrepo.GetById(lessonId);

            if (lesson == null)
            {
                return (false, "Lesson not found.", null);
            }

            // 2. Check if the student is enrolled in the Track
            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == lesson.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            if (enrollment == null)
            {
                return (false, "You are not enrolled in this Track.", null);
            }

            // 3. If this is not the first lesson,
            //    find the previous lesson
            if (lesson.Order > 1)
            {
                var previousLesson = _unitOfWork.Lessonrepo
                    .GetByCondition(l =>
                        l.TrackId == lesson.TrackId &&
                        l.Order == lesson.Order - 1)
                    .FirstOrDefault();

                if (previousLesson == null)
                {
                    return (false, "Previous lesson not found.", null);
                }

                // 4. Check if the student completed the previous lesson
                var previousProgress = _unitOfWork.LessonProgressrepo
                    .GetByCondition(p =>
                        p.StudentId == studentId &&
                        p.LessonId == previousLesson.Id &&
                        p.IsCompleted)
                    .FirstOrDefault();

                if (previousProgress == null)
                {
                    return (
                        false,
                        "You must complete the previous lesson first.",
                        null
                    );
                }
            }

            // 5. Map Entity → DTO
            var lessonDto = _mapper.Map<LessonResponseDTO>(lesson);

            // 6. Everything is okay
            return (true, "Access granted.", lessonDto);
        }
    }
}