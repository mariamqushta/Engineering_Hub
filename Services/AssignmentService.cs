using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly UnitWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly IInstructorAuthorizationService
            _instructorAuthorizationService;

        private readonly ILessonCompletionService
            _lessonCompletionService;

        private readonly ITrackCompletionService
            _trackCompletionService;

        public AssignmentService(
            UnitWork unitWork,
            IMapper mapper,
            IInstructorAuthorizationService instructorAuthorizationService,
            ILessonCompletionService lessonCompletionService,
            ITrackCompletionService trackCompletionService)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;

            _instructorAuthorizationService =
                instructorAuthorizationService;

            _lessonCompletionService =
                lessonCompletionService;

            _trackCompletionService =
                trackCompletionService;
        }

        // ============================================================
        // CREATE ASSIGNMENT
        // ============================================================

        public async Task<(
            bool Success,
            string Message,
            AssignmentResponseDTO? Data)>
            CreateAssignmentAsync(
                int lessonId,
                AssignmentCreateDTO dto,
                string instructorId)
        {
            // 1. Find lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(lessonId);

            if (lesson == null)
            {
                return (
                    false,
                    "Lesson not found.",
                    null
                );
            }

            // 2. Check instructor manages this Track
            bool canManage =
                _instructorAuthorizationService
                    .CanManageTrack(
                        lesson.TrackId,
                        instructorId);

            if (!canManage)
            {
                return (
                    false,
                    "You are not assigned to this Track.",
                    null
                );
            }

            // 3. Map DTO to entity
            var assignment =
                _mapper.Map<Assignment>(dto);

            assignment.LessonId = lessonId;

            // 4. Save
            _unitOfWork.Assignmentrepo
                .add(assignment);

            await _unitOfWork.SaveAsync();

            // 5. Map entity to response DTO
            var result =
                _mapper.Map<AssignmentResponseDTO>(
                    assignment);

            return (
                true,
                "Assignment created successfully.",
                result
            );
        }

        // ============================================================
        // GET ASSIGNMENTS FOR LESSON
        // ============================================================

        public async Task<List<AssignmentResponseDTO>>
            GetAssignmentsForLessonAsync(
                int lessonId,
                string studentId)
        {
            // 1. Find lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(lessonId);

            if (lesson == null)
            {
                return new List<AssignmentResponseDTO>();
            }

            // 2. Check active enrollment
            var enrollment =
                _unitOfWork.TrackEnrollmentrepo
                    .GetByCondition(e =>
                        e.StudentId == studentId &&
                        e.TrackId == lesson.TrackId &&
                        e.Status == SessionStatus.Active)
                    .FirstOrDefault();

            if (enrollment == null)
            {
                return new List<AssignmentResponseDTO>();
            }

            // 3. Check previous lessons
            if (lesson.Order > 1)
            {
                var previousLessons =
                    _unitOfWork.Lessonrepo
                        .GetByCondition(l =>
                            l.TrackId == lesson.TrackId &&
                            l.Order < lesson.Order);

                foreach (var previousLesson in previousLessons)
                {
                    var progress =
                        _unitOfWork.LessonProgressrepo
                            .GetByCondition(p =>
                                p.StudentId == studentId &&
                                p.LessonId == previousLesson.Id &&
                                p.IsCompleted)
                            .FirstOrDefault();

                    if (progress == null)
                    {
                        return new List<AssignmentResponseDTO>();
                    }
                }
            }

            // 4. Get assignments
            var assignments =
                _unitOfWork.Assignmentrepo
                    .GetByCondition(a =>
                        a.LessonId == lessonId);

            // 5. Map to response DTO
            return _mapper.Map<List<AssignmentResponseDTO>>(
                assignments);
        }

        // ============================================================
        // SUBMIT ASSIGNMENT
        // ============================================================

        public async Task<(bool Success, string Message)>
            SubmitAssignmentAsync(
                int assignmentId,
                AssignmentSubmissionDTO dto,
                string studentId)
        {
            // 1. Find assignment
            var assignment =
                _unitOfWork.Assignmentrepo
                    .GetById(assignmentId);

            if (assignment == null)
            {
                return (
                    false,
                    "Assignment not found."
                );
            }

            // 2. Find lesson
            var lesson =
                _unitOfWork.Lessonrepo
                    .GetById(assignment.LessonId);

            if (lesson == null)
            {
                return (
                    false,
                    "Lesson not found."
                );
            }

            // 3. Check active enrollment
            var enrollment =
                _unitOfWork.TrackEnrollmentrepo
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

            // 4. Check previous lessons
            if (lesson.Order > 1)
            {
                var previousLessons =
                    _unitOfWork.Lessonrepo
                        .GetByCondition(l =>
                            l.TrackId == lesson.TrackId &&
                            l.Order < lesson.Order);

                foreach (var previousLesson in previousLessons)
                {
                    var progress =
                        _unitOfWork.LessonProgressrepo
                            .GetByCondition(p =>
                                p.StudentId == studentId &&
                                p.LessonId == previousLesson.Id &&
                                p.IsCompleted)
                            .FirstOrDefault();

                    if (progress == null)
                    {
                        return (
                            false,
                            "You must complete the previous lessons first."
                        );
                    }
                }
            }

            // 5. Validate submission
            if (string.IsNullOrWhiteSpace(dto.Content) &&
                string.IsNullOrWhiteSpace(dto.FileUrl))
            {
                return (
                    false,
                    "You must provide either content or a file."
                );
            }

            // 6. Create submission
            var submission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                Content = dto.Content,
                FileUrl = dto.FileUrl,
                SubmittedAt = DateTime.UtcNow
            };

            _unitOfWork.AssignmentSubmissionrepo
                .add(submission);

            // 7. Save submission
            await _unitOfWork.SaveAsync();

            // 8. Try to complete the lesson
            var completionResult =
                await _lessonCompletionService
                    .CompleteLessonAsync(
                        lesson.Id,
                        studentId);

            if (completionResult.Success)
            {
                return (
                    true,
                    "Assignment submitted successfully. Lesson completed."
                );
            }

            if (completionResult.Message ==
                "You must submit all assignments before completing this lesson.")
            {
                return (
                    true,
                    "Assignment submitted successfully. Submit the remaining assignments to complete the lesson."
                );
            }

            if (completionResult.Message ==
                "All assignments must be graded before completing this lesson.")
            {
                return (
                    true,
                    "Assignment submitted successfully. Waiting for all assignments to be graded."
                );
            }

            return (
                true,
                "Assignment submitted successfully."
            );
        }

        // ============================================================
        // GET SUBMISSIONS
        // ============================================================

        public async Task<List<AssignmentSubmissionResponseDTO>>
            GetSubmissionsAsync(
                int assignmentId,
                string instructorId)
        {
            // 1. Find assignment
            var assignment =
                _unitOfWork.Assignmentrepo
                    .GetById(assignmentId);

            if (assignment == null)
            {
                return new List<AssignmentSubmissionResponseDTO>();
            }

            // 2. Find lesson
            var lesson =
                _unitOfWork.Lessonrepo
                    .GetById(assignment.LessonId);

            if (lesson == null)
            {
                return new List<AssignmentSubmissionResponseDTO>();
            }

            // 3. Check instructor access
            bool canManage =
                _instructorAuthorizationService
                    .CanManageTrack(
                        lesson.TrackId,
                        instructorId);

            if (!canManage)
            {
                return new List<AssignmentSubmissionResponseDTO>();
            }

            // 4. Get submissions
            var submissions =
                _unitOfWork.AssignmentSubmissionrepo
                    .GetByCondition(s =>
                        s.AssignmentId == assignmentId);

            // 5. Map to response DTO
            return _mapper.Map<
                List<AssignmentSubmissionResponseDTO>>(
                    submissions);
        }

        // ============================================================
        // GRADE SUBMISSION
        // ============================================================

        public async Task<(bool Success, string Message)>
            GradeSubmissionAsync(
                int submissionId,
                decimal grade,
                string? feedback,
                string instructorId)
        {
            // 1. Find submission
            var submission =
                _unitOfWork.AssignmentSubmissionrepo
                    .GetById(submissionId);

            if (submission == null)
            {
                return (
                    false,
                    "Submission not found."
                );
            }

            // 2. Find assignment
            var assignment =
                _unitOfWork.Assignmentrepo
                    .GetById(submission.AssignmentId);

            if (assignment == null)
            {
                return (
                    false,
                    "Assignment not found."
                );
            }

            // 3. Find lesson
            var lesson =
                _unitOfWork.Lessonrepo
                    .GetById(assignment.LessonId);

            if (lesson == null)
            {
                return (
                    false,
                    "Lesson not found."
                );
            }

            // 4. Check instructor manages this Track
            bool canManage =
                _instructorAuthorizationService
                    .CanManageTrack(
                        lesson.TrackId,
                        instructorId);

            if (!canManage)
            {
                return (
                    false,
                    "You are not assigned to this Track."
                );
            }

            // 5. Validate grade
            if (grade < 0 || grade > 100)
            {
                return (
                    false,
                    "Grade must be between 0 and 100."
                );
            }

            // 6. Update grade and feedback
            submission.Grade = grade;
            submission.Feedback = feedback;

            _unitOfWork.AssignmentSubmissionrepo
                .Edit(submission);

            // 7. Save grade
            await _unitOfWork.SaveAsync();

            // 8. Try to complete the lesson
            await _lessonCompletionService
                .CompleteLessonAsync(
                    lesson.Id,
                    submission.StudentId);

            // 9. Check Track completion
            await _trackCompletionService
                .CheckAndCompleteTrackAsync(
                    lesson.TrackId,
                    submission.StudentId);

            return (
                true,
                "Submission graded successfully."
            );
        }

        // ============================================================
        // GET MY SUBMISSION
        // ============================================================

        public async Task<AssignmentSubmissionResponseDTO?>
            GetMySubmissionAsync(
                int assignmentId,
                string studentId)
        {
            // 1. Find assignment
            var assignment =
                _unitOfWork.Assignmentrepo
                    .GetById(assignmentId);

            if (assignment == null)
            {
                return null;
            }

            // 2. Find lesson
            var lesson =
                _unitOfWork.Lessonrepo
                    .GetById(assignment.LessonId);

            if (lesson == null)
            {
                return null;
            }

            // 3. Check active enrollment
            var enrollment =
                _unitOfWork.TrackEnrollmentrepo
                    .GetByCondition(e =>
                        e.StudentId == studentId &&
                        e.TrackId == lesson.TrackId &&
                        e.Status == SessionStatus.Active)
                    .FirstOrDefault();

            if (enrollment == null)
            {
                return null;
            }

            // 4. Check previous lessons
            if (lesson.Order > 1)
            {
                var previousLessons =
                    _unitOfWork.Lessonrepo
                        .GetByCondition(l =>
                            l.TrackId == lesson.TrackId &&
                            l.Order < lesson.Order);

                foreach (var previousLesson in previousLessons)
                {
                    var progress =
                        _unitOfWork.LessonProgressrepo
                            .GetByCondition(p =>
                                p.StudentId == studentId &&
                                p.LessonId == previousLesson.Id &&
                                p.IsCompleted)
                            .FirstOrDefault();

                    if (progress == null)
                    {
                        return null;
                    }
                }
            }

            // 5. Find latest submission
            var submission =
                _unitOfWork.AssignmentSubmissionrepo
                    .GetByCondition(s =>
                        s.AssignmentId == assignmentId &&
                        s.StudentId == studentId)
                    .OrderByDescending(s => s.SubmittedAt)
                    .FirstOrDefault();

            if (submission == null)
            {
                return null;
            }

            // 6. Map to response DTO
            return _mapper.Map<
                AssignmentSubmissionResponseDTO>(
                    submission);
        }
    }
}