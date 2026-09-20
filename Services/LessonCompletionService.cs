using Engineering_Hub.models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Engineering_Hub.UnitOfWork;

namespace Engineering_Hub.Services
{
    public class LessonCompletionService : ILessonCompletionService
    {
        private readonly UnitWork _unitOfWork;

        public LessonCompletionService(UnitWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message)> CompleteLessonAsync(
    int lessonId,
    string studentId)
        {
            // 1. Find the lesson
            var lesson = _unitOfWork.Lessonrepo.GetById(lessonId);

            if (lesson == null)
            {
                return (false, "Lesson not found.");
            }

            // 2. Check if student is enrolled in the Track
            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == lesson.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            if (enrollment == null)
            {
                return (false, "You are not enrolled in this Track.");
            }

            // 3. Check previous lesson
            if (lesson.Order > 1)
            {
                var previousLesson = _unitOfWork.Lessonrepo
                    .GetByCondition(l =>
                        l.TrackId == lesson.TrackId &&
                        l.Order == lesson.Order - 1)
                    .FirstOrDefault();

                if (previousLesson == null)
                {
                    return (false, "Previous lesson not found.");
                }

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
                        "You must complete the previous lesson first."
                    );
                }
            }

            // 4. Find current lesson progress
            var progress = _unitOfWork.LessonProgressrepo
                .GetByCondition(p =>
                    p.StudentId == studentId &&
                    p.LessonId == lessonId)
                .FirstOrDefault();

            // 5. Create or update progress
            if (progress != null)
            {
                if (progress.IsCompleted)
                {
                    return (false, "This lesson is already completed.");
                }

                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;

                _unitOfWork.LessonProgressrepo.Edit(progress);
            }
            else
            {
                progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                _unitOfWork.LessonProgressrepo.add(progress);
            }

            // 6. Save
          
            await _unitOfWork.SaveAsync();

            // Check if all lessons are completed
            var allLessons = _unitOfWork.Lessonrepo
                .GetByCondition(l => l.TrackId == lesson.TrackId);

            var completedLessons = _unitOfWork.LessonProgressrepo
                .GetByCondition(p =>
                    p.StudentId == studentId &&
                    p.IsCompleted);

            bool allLessonsCompleted = allLessons
                .All(l => completedLessons.Any(p => p.LessonId == l.Id));

            if (allLessonsCompleted)
            {
                enrollment.Status = SessionStatus.Completed;
                enrollment.CompletedAt = DateTime.UtcNow;

                _unitOfWork.TrackEnrollmentrepo.Edit(enrollment);

                await _unitOfWork.SaveAsync();
            }

            return (true, "Lesson completed successfully.");
        }
    }
}