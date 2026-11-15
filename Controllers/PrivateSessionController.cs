
using Engineering_Hub.DTOs.PrivateSessions;
using Engineering_Hub.models;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/private-sessions")]
    public class PrivateSessionController : ControllerBase
    {
        private readonly IPrivateSessionService _privateSessions;
        private readonly UserManager<ApplicationUser> _userManager;

        public PrivateSessionController(
            IPrivateSessionService privateSessions,
            UserManager<ApplicationUser> userManager)
        {
            _privateSessions = privateSessions;
            _userManager = userManager;
        }

        private IActionResult PrivateError(Exception ex) => ex switch
        {
            KeyNotFoundException => NotFound(new { message = ex.Message }),
            UnauthorizedAccessException => Forbid(),
            ArgumentException or InvalidOperationException =>
                BadRequest(new { message = ex.Message }),
            _ => StatusCode(500, new { message = "An unexpected error occurred." })
        };

        // ---------- Student ----------

        [HttpGet("student/sessions")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMySessions(
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null ||
                    !user.IsActive ||
                    user.AccountType != AccountType.PrivateStudent)
                    return Forbid();

                var from = fromUtc ?? DateTime.UtcNow;
                var to = toUtc ?? DateTime.UtcNow.AddDays(84);

                var sessions = await _privateSessions.GetStudentOccurrencesAsync(
                    user.Id, from, to);

                return Ok(sessions.Select(s => new StudentPrivateOccurrenceDto
                {
                    Id = s.Id,
                    InstructorName = s.Instructor?.FullName ?? "",
                    TrackId = s.TrackId,
                    TrackTitle = s.Track?.Title ?? "",
                    StartUtc = s.StartUtc,
                    EndUtc = s.EndUtc,
                    Status = s.Status,
                    IsReplacement = s.Replacement != null
                }));
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        [HttpPost("student/sessions/{occurrenceId:int}/cancel")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CancelSession(int occurrenceId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null ||
                !user.IsActive ||
                user.AccountType != AccountType.PrivateStudent)
                return Forbid();

            try
            {
                await _privateSessions.CancelOccurrenceAsync(occurrenceId, user.Id);
                return NoContent();
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // ---------- Instructor ----------

        [HttpGet("instructor/sessions")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorSessions(
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc)
        {
            try
            {
                var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(instructorId))
                    return Unauthorized();

                var instructor = await _userManager.FindByIdAsync(instructorId);

                if (instructor == null || !instructor.IsActive)
                    return Forbid();

                var from = fromUtc ?? DateTime.UtcNow;
                var to = toUtc ?? DateTime.UtcNow.AddDays(84);

                var sessions = await _privateSessions.GetInstructorOccurrencesAsync(
                    instructorId, from, to);

                return Ok(sessions.Select(s => new InstructorPrivateOccurrenceDto
                {
                    Id = s.Id,
                    StudentName = s.Status == SessionStatus.Cancelled
                        ? null
                        : s.Student?.FullName,
                    TrackId = s.TrackId,
                    TrackTitle = s.Track?.Title ?? "",
                    StartUtc = s.StartUtc,
                    EndUtc = s.EndUtc,
                    Status = s.Status,
                    IsReplacement = s.Replacement != null
                }));
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // GET: api/private-sessions/instructor/availability
        [HttpGet("instructor/availability")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyAvailability()
        {
            try
            {
                var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(instructorId))
                    return Unauthorized();

                var instructor = await _userManager.FindByIdAsync(instructorId);

                if (instructor == null || !instructor.IsActive)
                    return Forbid();

                var slots = await _privateSessions.GetInstructorAvailabilityAsync(
                    instructorId);

                return Ok(slots.Select(s => new PrivateAvailabilityDto
                {
                    Id = s.Id,
                    InstructorId = s.InstructorId,
                    InstructorName = instructor.FullName,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    ValidFrom = s.ValidFrom,
                    ValidTo = s.ValidTo,
                    IsActive = s.IsActive
                }));
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // POST: api/private-sessions/instructor/sessions/{occurrenceId}/absence
        [HttpPost("instructor/sessions/{occurrenceId:int}/absence")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> ReportStudentAbsence(int occurrenceId)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized();

            try
            {
                var request = await _privateSessions.ReportStudentAbsenceAsync(
                    instructorId, occurrenceId);

                return Ok(new
                {
                    request.Id,
                    request.OccurrenceId,
                    request.RequestType,
                    request.Status,
                    request.CreatedAtUtc
                });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // POST: api/private-sessions/instructor/sessions/{occurrenceId}/cancellation-request
        [HttpPost("instructor/sessions/{occurrenceId:int}/cancellation-request")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> RequestSessionCancellation(int occurrenceId)
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized();

            try
            {
                var request = await _privateSessions.RequestInstructorCancellationAsync(
                    instructorId, occurrenceId);

                return Ok(new
                {
                    request.Id,
                    request.OccurrenceId,
                    request.RequestType,
                    request.Status,
                    request.CreatedAtUtc
                });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }
    }
}
