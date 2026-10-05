using Engineering_Hub.DTO;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

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

            return Ok(result.Message);
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
    }
}