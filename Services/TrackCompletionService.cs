using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class TrackCompletionService : ITrackCompletionService
    {
        private readonly UnitWork _unitOfWork;

        public TrackCompletionService(UnitWork unitWork)
        {
            _unitOfWork = unitWork;
        }

        public async Task<bool> CheckAndCompleteTrackAsync(
            int trackId,
            string studentId)
        {
            // 1. Find active enrollment
            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == trackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            if (enrollment == null)
            {
                return false;
            }

            // 2. Get all lessons in the Track
            var lessons = _unitOfWork.Lessonrepo
                .GetByCondition(l =>
                    l.TrackId == trackId);

            // 3. All lessons must be completed
            var completedLessons =
                _unitOfWork.LessonProgressrepo
                    .GetByCondition(p =>
                        p.StudentId == studentId &&
                        p.IsCompleted);

            bool allLessonsCompleted = lessons.All(lesson =>
                completedLessons.Any(progress =>
                    progress.LessonId == lesson.Id));

            if (!allLessonsCompleted)
            {
                return false;
            }

            // 4. Get the lesson IDs
            var lessonIds = lessons
                .Select(l => l.Id)
                .ToList();

            // 5. Get all assignments belonging to these lessons
            var assignments =
                _unitOfWork.Assignmentrepo
                    .GetByCondition(a =>
                        lessonIds.Contains(a.LessonId));

            // 6. Every assignment must be submitted and graded
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
                    return false;
                }

                if (submission.Grade == null)
                {
                    return false;
                }
            }

            // 7. Everything is completed
            enrollment.Status = SessionStatus.Completed;
            enrollment.CompletedAt = DateTime.UtcNow;

            // 8. Check if the student already has a certificate
            var existingCertificate =
                _unitOfWork.Certificaterepo
                    .GetByCondition(c =>
                        c.StudentId == studentId &&
                        c.TrackId == trackId)
                    .FirstOrDefault();

            // 9. Create certificate if one does not already exist
            if (existingCertificate == null)
            {
                var certificate = new Certificate
                {
                    StudentId = studentId,
                    TrackId = trackId,
                    IssuedAt = DateTime.UtcNow
                };

                _unitOfWork.Certificaterepo.add(certificate);
            }

            // 10. Save everything
            _unitOfWork.TrackEnrollmentrepo
                .Edit(enrollment);

            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}