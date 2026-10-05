
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class LessonCompletionService : ILessonCompletionService
    {
        private readonly UnitWork _unitOfWork;
        private readonly ITrackCompletionService _trackCompletionService;

        public LessonCompletionService(
            UnitWork unitWork,
            ITrackCompletionService trackCompletionService)
        {
            _unitOfWork = unitWork;
            _trackCompletionService = trackCompletionService;
        }

        public async Task<(bool Success, string Message)> CompleteLessonAsync(
            int lessonId,
            string studentId)
        {
            // 1. Find the lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(lessonId);

            if (lesson == null)
            {
                return (
                    false,
                    "Lesson not found."
                );
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
                return (
                    false,
                    "You are not enrolled in this Track."
                );
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
                    return (
                        false,
                        "Previous lesson not found."
                    );
                }

                var previousProgress =
                    _unitOfWork.LessonProgressrepo
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

            // 4. Check assignments
            var assignments =
                _unitOfWork.Assignmentrepo
                    .GetByCondition(a =>
                        a.LessonId == lessonId);

            // All assignments must be submitted and graded.
            foreach (var assignment in assignments)
            {
                var submission =
                    _unitOfWork.AssignmentSubmissionrepo
                        .GetByCondition(s =>
                            s.AssignmentId == assignment.Id &&
                            s.StudentId == studentId)
                        .FirstOrDefault();

                if (submission == null)
                {
                    return (
                        false,
                        "You must submit all assignments before completing this lesson."
                    );
                }

                if (submission.Grade == null)
                {
                    return (
                        false,
                        "All assignments must be graded before completing this lesson."
                    );
                }
            }

            // 5. Find current lesson progress
            var progress = _unitOfWork.LessonProgressrepo
                .GetByCondition(p =>
                    p.StudentId == studentId &&
                    p.LessonId == lessonId)
                .FirstOrDefault();

            // 6. Create or update progress
            if (progress != null)
            {
                if (progress.IsCompleted)
                {
                    return (
                        false,
                        "This lesson is already completed."
                    );
                }

                progress.IsCompleted = true;
                progress.CompletedAt = System.DateTime.UtcNow;

                _unitOfWork.LessonProgressrepo.Edit(progress);
            }
            else
            {
                progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = System.DateTime.UtcNow
                };

                _unitOfWork.LessonProgressrepo.add(progress);
            }

            // 7. Save lesson progress
            await _unitOfWork.SaveAsync();

            // 8. Check Track completion
            await _trackCompletionService.CheckAndCompleteTrackAsync(
                lesson.TrackId,
                studentId);

            return (
                true,
                "Lesson completed successfully."
            );
        }
    }
}
