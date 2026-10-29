using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class TrackCompletionService : ITrackCompletionService
    {
        private readonly UnitWork _unitOfWork;
        private readonly ICertificatePdfService _certificatePdfService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public TrackCompletionService(
            UnitWork unitWork,
            ICertificatePdfService certificatePdfService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _unitOfWork = unitWork;
            _certificatePdfService = certificatePdfService;
            _userManager = userManager;
            _environment = environment;
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

            // 2. Get all lessons in the track
            var lessons = _unitOfWork.Lessonrepo
                .GetByCondition(l => l.TrackId == trackId)
                .ToList();

            // 3. All lessons must be completed
            var completedLessons = _unitOfWork.LessonProgressrepo
                .GetByCondition(p =>
                    p.StudentId == studentId &&
                    p.IsCompleted)
                .ToList();

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
            var assignments = _unitOfWork.Assignmentrepo
                .GetByCondition(a =>
                    lessonIds.Contains(a.LessonId));

            // 6. Every assignment must be submitted and graded
            foreach (var assignment in assignments)
            {
                var submission = _unitOfWork.AssignmentSubmissionrepo
                    .GetByCondition(s =>
                        s.AssignmentId == assignment.Id &&
                        s.StudentId == studentId)
                    .FirstOrDefault();

                if (submission == null || submission.Grade == null)
                {
                    return false;
                }
            }

            // 7. Check quiz requirements
            foreach (var lesson in lessons)
            {
                var endQuiz = _unitOfWork.LessonQuizrepo
                    .GetByCondition(q =>
                        q.LessonId == lesson.Id &&
                        q.Type == QuizType.EndOfLesson &&
                        q.IsActive)
                    .FirstOrDefault();

                if (endQuiz == null)
                {
                    return false;
                }

                bool passedEndQuiz = _unitOfWork.QuizAttemptrepo
                    .GetByCondition(a =>
                        a.LessonQuizId == endQuiz.Id &&
                        a.StudentId == studentId &&
                        a.Passed)
                    .Any();

                if (passedEndQuiz)
                {
                    continue;
                }

                // Check whether a later lesson's prerequisite quiz was passed
                bool passedLaterPrerequisite = lessons
                    .Where(l =>
                        l.TrackId == trackId &&
                        l.Order > lesson.Order)
                    .Any(l =>
                    {
                        var prerequisiteQuiz = _unitOfWork.LessonQuizrepo
                            .GetByCondition(q =>
                                q.LessonId == l.Id &&
                                q.Type == QuizType.Prerequisite &&
                                q.IsActive)
                            .FirstOrDefault();

                        if (prerequisiteQuiz == null)
                        {
                            return false;
                        }

                        return _unitOfWork.QuizAttemptrepo
                            .GetByCondition(a =>
                                a.LessonQuizId == prerequisiteQuiz.Id &&
                                a.StudentId == studentId &&
                                a.Passed)
                            .Any();
                    });

                if (!passedLaterPrerequisite)
                {
                    return false;
                }
            }

            // 8. Everything is completed
            enrollment.Status = SessionStatus.Completed;
            enrollment.CompletedAt = DateTime.UtcNow;

            // 9. Find an existing certificate
            var certificate = _unitOfWork.Certificaterepo
                .GetByCondition(c =>
                    c.StudentId == studentId &&
                    c.TrackId == trackId)
                .FirstOrDefault();

            // 10. Create a certificate if one does not exist
            if (certificate == null)
            {
                certificate = new Certificate
                {
                    StudentId = studentId,
                    TrackId = trackId,
                    IssuedAt = DateTime.UtcNow
                };

                _unitOfWork.Certificaterepo.add(certificate);
            }

            // 11. Save the enrollment and certificate first.
            // This ensures a new certificate has its database ID.
            _unitOfWork.TrackEnrollmentrepo.Edit(enrollment);

            await _unitOfWork.SaveAsync();

            // 12. Generate the PDF if it does not already exist
            var certificatesDirectory = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "certificates");

            Directory.CreateDirectory(certificatesDirectory);

            var pdfPath = Path.Combine(
                certificatesDirectory,
                $"certificate-{certificate.Id}.pdf");

            if (!File.Exists(pdfPath) ||
                string.IsNullOrWhiteSpace(certificate.CertificateUrl))
            {
                var student = await _userManager.FindByIdAsync(studentId);
                var track = _unitOfWork.Trackrepo.GetById(trackId);

                if (student == null || track == null)
                {
                    throw new InvalidOperationException(
                        "The student or track could not be found for certificate generation.");
                }

                certificate.Student = student;
                certificate.Track = track;

                var pdfBytes = _certificatePdfService.Generate(certificate);

                await File.WriteAllBytesAsync(pdfPath, pdfBytes);

                // Store the API download route, not the physical file path.
                certificate.CertificateUrl =
                    $"/api/Certificate/{certificate.Id}/download";

                _unitOfWork.Certificaterepo.Edit(certificate);

                await _unitOfWork.SaveAsync();
            }

            return true;
        }

        public async Task<bool> IsTrackCompletedAsync(
            int trackId,
            string studentId)
        {
            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.TrackId == trackId &&
                    e.StudentId == studentId &&
                    e.Status == SessionStatus.Completed)
                .FirstOrDefault();

            return enrollment != null &&
                   enrollment.CompletedAt.HasValue;
        }
    }
}