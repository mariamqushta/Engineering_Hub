
using Engineering_Hub.DTOs.Quiz;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // INSTRUCTOR - Create a quiz for a lesson
        [HttpPost("lesson/{lessonId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateQuiz(
            int lessonId,
            [FromBody] CreateLessonQuizDto dto)
        {
            var instructorId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized();

            var result = await _quizService.CreateQuizAsync(
                lessonId,
                instructorId,
                dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }

        // INSTRUCTOR - Get quizzes for a lesson
        [HttpGet("lesson/{lessonId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetLessonQuizzes(
            int lessonId)
        {
            var instructorId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized();

            var quizzes = await _quizService.GetLessonQuizzesAsync(
                lessonId,
                instructorId);

            return Ok(quizzes);
        }

        // STUDENT - Get a quiz
        [HttpGet("{quizId}/student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentQuiz(int quizId)
        {
            var studentId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var quiz = await _quizService.GetStudentQuizAsync(
                quizId, studentId);

            if (quiz == null)
                return NotFound(new
                {
                    message = "Quiz not found or not available."
                });

            return Ok(quiz);
        }

        // STUDENT - Submit quiz answers
        [HttpPost("{quizId}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitQuiz(
            int quizId,
            [FromBody] SubmitQuizDto dto)
        {
            var studentId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var result = await _quizService.SubmitQuizAsync(
                quizId, studentId, dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                result = result.Data
            });
        }

        // STUDENT - Get available quizzes for a lesson
        [HttpGet("lesson/{lessonId}/student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentLessonQuizzes(
            int lessonId)
        {
            var studentId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var quizzes =
                await _quizService.GetStudentLessonQuizzesAsync(
                    lessonId,
                    studentId);

            return Ok(quizzes);
        }

    }
}
