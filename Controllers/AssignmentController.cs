using Engineering_Hub.DTO;
using Engineering_Hub.DTOs;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(
            IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }


        // =====================================================
        // INSTRUCTOR - Create Assignment
        // =====================================================

        [HttpPost("lesson/{lessonId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateAssignment(
            int lessonId,
            AssignmentCreateDTO dto)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result =
                await _assignmentService.CreateAssignmentAsync(
                    lessonId,
                    dto,
                    instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }


        // =====================================================
        // STUDENT - Get Assignments For Lesson
        // =====================================================

        [HttpGet("lesson/{lessonId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAssignmentsForLesson(
            int lessonId)
        {
            var studentId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (studentId == null)
                return Unauthorized();

            var assignments =
                await _assignmentService
                    .GetAssignmentsForLessonAsync(
                        lessonId,
                        studentId);

            return Ok(assignments);
        }


        // =====================================================
        // STUDENT - Submit Assignment
        // =====================================================

        [HttpPost("{assignmentId}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAssignment(
            int assignmentId,
            AssignmentSubmissionDTO dto)
        {
            var studentId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (studentId == null)
                return Unauthorized();

            var result =
                await _assignmentService
                    .SubmitAssignmentAsync(
                        assignmentId,
                        dto,
                        studentId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }


        // =====================================================
        // INSTRUCTOR - Get Submissions
        // =====================================================

        [HttpGet("{assignmentId}/submissions")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetSubmissions(
            int assignmentId)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var submissions =
                await _assignmentService
                    .GetSubmissionsAsync(
                        assignmentId,
                        instructorId);

            return Ok(submissions);
        }


        // =====================================================
        // INSTRUCTOR - Grade Submission
        // =====================================================

        [HttpPut("submissions/{submissionId}/grade")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GradeSubmission(
            int submissionId,
            GradeSubmissionDTO dto)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result =
                await _assignmentService
                    .GradeSubmissionAsync(
                        submissionId,
                        dto.Grade,
                        dto.Feedback,
                        instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }
        // =====================================================
        // STUDENT - Get My Submission
        // =====================================================

        [HttpGet("{assignmentId}/my-submission")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMySubmission(
            int assignmentId)
        {
            var studentId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (studentId == null)
                return Unauthorized();

            var submission =
                await _assignmentService
                    .GetMySubmissionAsync(
                        assignmentId,
                        studentId);

            if (submission == null)
                return NotFound(
                    "No submission found.");

            return Ok(submission);
        }

        [HttpPost("lesson/{lessonId}/with-file")]
        [Authorize(Roles = "Instructor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateAssignmentWithFile(
    int lessonId,
    [FromForm] AssignmentCreateFormDTO dto)
        {
            var instructorId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized();

            var result = await _assignmentService.CreateAssignmentWithFileAsync(
                lessonId,
                dto,
                instructorId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }

        [HttpPost("{assignmentId}/submit-with-file")]
        [Authorize(Roles = "Student")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitAssignmentWithFile(
    int assignmentId,
    [FromForm] AssignmentSubmissionFormDTO dto)
        {
            var studentId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var result = await _assignmentService.SubmitAssignmentWithFileAsync(
                assignmentId,
                dto,
                studentId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
       }
        [HttpGet("instructor/lesson/{lessonId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorAssignmentsForLesson(
    int lessonId)
        {
            var instructorId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var assignments =
             await _assignmentService.GetAssignmentsForInstructorLessonAsync(
                 lessonId,
                 instructorId);

            return Ok(assignments);
        }

        // STUDENT - Download assignment reference attachment
        [HttpGet("{assignmentId}/attachment")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DownloadAssignmentAttachment(
            int assignmentId)
        {
            var studentId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var result = await _assignmentService
                .GetAssignmentAttachmentAsync(
                    assignmentId,
                    studentId);

            if (!result.Success || result.FilePath == null)
                return NotFound(result.Message);

            return PhysicalFile(
                result.FilePath,
                result.ContentType ?? "application/octet-stream",
                result.FileName ?? "assignment-file");
        }

        // STUDENT or INSTRUCTOR - Download a submitted file
        [HttpGet("submissions/{submissionId}/file")]
        [Authorize(Roles = "Student,Instructor")]
        public async Task<IActionResult> DownloadSubmissionFile(
            int submissionId)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            bool isInstructor = User.IsInRole("Instructor");

            var result = await _assignmentService
                .GetSubmissionFileAsync(
                    submissionId,
                    userId,
                    isInstructor);

            if (!result.Success)
            {
                if (result.Message.Contains("cannot access",
                        StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("not assigned",
                        StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("not enrolled",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                if (result.Message.Contains("previous lessons",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }

                return NotFound(result.Message);
            }

            if (result.FilePath == null)
                return NotFound("File not found.");

            return PhysicalFile(
                result.FilePath,
                result.ContentType ?? "application/octet-stream",
                result.FileName ?? "submission-file");
        }

    }
}