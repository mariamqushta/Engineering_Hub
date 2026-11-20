using Engineering_Hub.DTO.Admin;
using Engineering_Hub.DTOs.PrivateSessions;
using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ITrackBookingService _trackBookingService;
        private readonly EngineeringHubContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPrivateSessionService _privateSessions;
        private readonly IWorkshopService _workshopService;
        private readonly IInteractiveActivityService _activityService;
        public AdminController(
       ITrackBookingService trackBookingService,
       EngineeringHubContext context,
       UserManager<ApplicationUser> userManager,
       IPrivateSessionService privateSessions,
       IWorkshopService workshopService,
       IInteractiveActivityService activityService)
        {
            _trackBookingService = trackBookingService;
            _context = context;
            _userManager = userManager;
            _privateSessions = privateSessions;
            _workshopService = workshopService;
            _activityService = activityService;
        }

        // GET: api/Admin/instructors
        [HttpGet("instructors")]
        public async Task<IActionResult> GetInstructors()
        {
            var instructors =
                await _userManager.GetUsersInRoleAsync("Instructor");

            var instructorIds = instructors
                .Select(x => x.Id)
                .ToList();

            var assignments = await _context.TrackInstructors
                .AsNoTracking()
                .Include(x => x.Track)
                .Where(x => instructorIds.Contains(x.UserId))
                .ToListAsync();

            var result = new
            {
                totalCount = instructors.Count,
                instructors = instructors.Select(user => new
                {
                    // Account information
                    user.Id,
                    user.UserName,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    user.LockoutEnabled,
                    user.LockoutEnd,
                    user.AccessFailedCount,

                    // Application-specific information
                    user.ProfileImageUrl,
                    user.Bio,
                    user.IsActive,
                    user.CreatedAt,

                    // Assigned tracks
                    tracks = assignments
                        .Where(x => x.UserId == user.Id)
                        .Select(x => new
                        {
                            x.TrackId,
                            Title = x.Track.Title,
                            x.Track.Description,
                            x.Track.IsActive
                        })
                        .ToList()
                }).ToList()
            };

            return Ok(result);
        }

        // GET: api/Admin/students
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            // Get every account with the Student role first.
            // Students without bookings must remain in the result.
            var students =
                await _userManager.GetUsersInRoleAsync("Student");

            var studentIds = students
                .Select(x => x.Id)
                .ToList();

            // Track enrollments
            var trackEnrollments = await _context.TrackEnrollments
                .AsNoTracking()
                .Include(x => x.Track)
                .Where(x => studentIds.Contains(x.StudentId))
                .ToListAsync();

            // Workshop bookings
            var workshopBookings = await _context.WorkshopBookings
                .AsNoTracking()
                .Include(x => x.Workshop)
                    .ThenInclude(x => x.Track)
                .Where(x => studentIds.Contains(x.UserId))
                .ToListAsync();

            // Interactive activity bookings
            var interactiveBookings = await _context.InteractiveBookings
                .AsNoTracking()
                .Include(x => x.InteractiveActivity)
                    .ThenInclude(x => x.Track)
                .Where(x => studentIds.Contains(x.UserId))
                .ToListAsync();

            var result = new
            {
                totalCount = students.Count,

                students = students.Select(user => new
                {
                    // Account information
                    user.Id,
                    user.UserName,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    user.LockoutEnabled,
                    user.LockoutEnd,
                    user.AccessFailedCount,

                    // Application-specific information
                    user.ProfileImageUrl,
                    user.Bio,
                    user.IsActive,
                    user.CreatedAt,
                    user.AccountType,
                    // Track enrollments
                    trackBookings = trackEnrollments
                        .Where(x => x.StudentId == user.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.TrackId,
                            TrackTitle = x.Track.Title,
                            x.EnrolledAt,
                            x.Status,
                            x.CompletedAt,
                            x.ApprovedPrice,
                            x.BookingType,
                            x.ApprovalStatus
                        })
                        .ToList(),

                    // Workshop bookings
                    workshopBookings = workshopBookings
                        .Where(x => x.UserId == user.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.WorkshopId,
                            WorkshopTitle = x.Workshop.Title,
                            TrackTitle = x.Workshop.Track.Title,
                            x.BookedAt,
                            x.Status,
                            x.Source,
                            x.ApprovalStatus,
                            x.Workshop.Date,
                            x.Workshop.StartTime,
                            x.Workshop.EndTime,
                            x.Workshop.Place
                        })
                        .ToList(),

                    // Interactive activity bookings
                    interactiveBookings = interactiveBookings
                        .Where(x => x.UserId == user.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.InteractiveActivityId,
                            ActivityTitle = x.InteractiveActivity.Title,
                            TrackTitle = x.InteractiveActivity.Track.Title,
                            x.BookedAt,
                            x.Status,
                            x.Source,
                            x.ApprovalStatus,
                            x.InteractiveActivity.Date,
                            x.InteractiveActivity.StartTime,
                            x.InteractiveActivity.EndTime
                        })
                        .ToList(),

                    // Convenient summary for the admin UI
                    totalTrackBookings = trackEnrollments
                        .Count(x => x.StudentId == user.Id),

                    totalWorkshopBookings = workshopBookings
                        .Count(x => x.UserId == user.Id),

                    totalInteractiveBookings = interactiveBookings
                        .Count(x => x.UserId == user.Id)
                }).ToList()
            };

            return Ok(result);
        }

        // GET: api/Admin/pending-bookings
        [HttpGet("pending-bookings")]
        public async Task<IActionResult> GetPendingBookings()
        {
            var requests =
                await _trackBookingService.GetPendingRequestsAsync();

            return Ok(requests);
        }

        // PUT: api/Admin/bookings/{id}/approve
        [HttpPut("bookings/{id}/approve")]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var result =
                await _trackBookingService.ApproveRequestAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // PUT: api/Admin/bookings/{id}/reject
        [HttpPut("bookings/{id}/reject")]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var result =
                await _trackBookingService.RejectRequestAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }// GET: api/Admin/workshops
        [HttpGet("workshops")]
        public async Task<IActionResult> GetAllWorkshops()
        {
            var workshops = await _context.Workshops
                .AsNoTracking()
                .Include(w => w.Track)
                .Include(w => w.Instructor)
                .Include(w => w.Bookings)
                    .ThenInclude(b => b.User)
                .Select(w => new
                {
                    id = w.Id,
                    title = w.Title,
                    description = w.Description,
                    trackTitle = w.Track.Title,
                    instructorUserName = w.Instructor != null
                        ? w.Instructor.UserName
                        : null,
                    date = w.Date,
                    startTime = w.StartTime,
                    endTime = w.EndTime,
                    place = w.Place,
                    capacity = w.Capacity,
                    status = w.Status,
                    approvalStatus = w.ApprovalStatus,
                    students = w.Bookings.Select(b => new
                    {
                        userName = b.User.UserName,
                        fullName = b.User.FullName,
                        email = b.User.Email,
                        bookingStatus = b.ApprovalStatus
                    }).ToList()
                })
                .ToListAsync();

            return Ok(workshops);
        }

        // PUT: api/Admin/workshops/{id}/approve
        [HttpPut("workshops/{id}/approve")]
        public async Task<IActionResult> ApproveWorkshop(int id)
        {
            var result = await _workshopService.AdminApproveWorkshopAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        // PUT: api/Admin/workshops/{id}/reject
        [HttpPut("workshops/{id}/reject")]
        public async Task<IActionResult> RejectWorkshop(int id)
        {
            var result = await _workshopService.AdminRejectWorkshopAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        // PUT: api/Admin/workshops/{id}/reschedule
        [HttpPut("workshops/{id}/reschedule")]
        public async Task<IActionResult> RescheduleWorkshop(
            int id,
            [FromBody] RescheduleActivityDTO request)
        {
            var result = await _workshopService.AdminRescheduleWorkshopAsync(
                id,
                request.Date,
                request.StartTime,
                request.EndTime);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        // PUT: api/Admin/workshops/{id}/cancel
        [HttpPut("workshops/{id}/cancel")]
        public async Task<IActionResult> CancelWorkshop(int id)
        {
            var result = await _workshopService.AdminCancelWorkshopAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        // GET: api/Admin/interactive-activities
        [HttpGet("interactive-activities")]
        public async Task<IActionResult> GetAllInteractiveActivities()
        {
            var activities = await _context.InteractiveActivities
                .AsNoTracking()
                .Include(a => a.Track)
                .Include(a => a.Instructor)
                .Include(a => a.Bookings)
                    .ThenInclude(b => b.User)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    description = a.Description,
                    trackTitle = a.Track.Title,
                    instructorUserName = a.Instructor != null
                        ? a.Instructor.UserName
                        : null,
                    date = a.Date,
                    startTime = a.StartTime,
                    endTime = a.EndTime,
                    meetingUrl = a.MeetingUrl,
                    capacity = a.Capacity,
                    status = a.Status,
                    approvalStatus = a.ApprovalStatus,
                    interactiveScheduleId = a.InteractiveScheduleId,
                    students = a.Bookings.Select(b => new
                    {
                        userName = b.User.UserName,
                        fullName = b.User.FullName,
                        email = b.User.Email,
                        bookingStatus = b.ApprovalStatus
                    }).ToList()
                })
                .ToListAsync();

            return Ok(activities);
        }



        // Approve one interactive activity
        [HttpPut("interactive-activities/{id}/approve")]
        public async Task<IActionResult> ApproveInteractiveActivity(int id)
        {
            var result = await _activityService.AdminApproveActivityAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // Reject one interactive activity
        [HttpPut("interactive-activities/{id}/reject")]
        public async Task<IActionResult> RejectInteractiveActivity(int id)
        {
            var result = await _activityService.AdminRejectActivityAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // Reschedule one interactive activity
        [HttpPut("interactive-activities/{id}/reschedule")]
        public async Task<IActionResult> RescheduleInteractiveActivity(
            int id,
            [FromBody] RescheduleActivityDTO request)
        {
            var result = await _activityService.AdminRescheduleActivityAsync(
                id, request.Date, request.StartTime, request.EndTime);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // Cancel one interactive activity
        [HttpPut("interactive-activities/{id}/cancel")]
        public async Task<IActionResult> CancelInteractiveActivity(int id)
        {
            var result = await _activityService.AdminCancelActivityAsync(id);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // Approve all pending activities in a recurring schedule
        [HttpPut("interactive-schedules/{scheduleId}/approve")]
        public async Task<IActionResult> ApproveInteractiveSchedule(int scheduleId)
        {
            var result = await _activityService.AdminApproveScheduleAsync(scheduleId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // Reject all activities in a recurring schedule
        [HttpPut("interactive-schedules/{scheduleId}/reject")]
        public async Task<IActionResult> RejectInteractiveSchedule(int scheduleId)
        {
            var result = await _activityService.AdminRejectScheduleAsync(scheduleId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
        // GET: api/Admin/pending-interactive-bookings




        [HttpGet("pending-interactive-bookings")]
        public async Task<IActionResult> GetPendingInteractiveBookings()
        {
            var requests = await _context.InteractiveBookings
                .AsNoTracking()
                .Where(b =>
                    b.Source == BookingSource.Individual &&
                    b.ApprovalStatus == BookingApprovalStatus.Pending &&
                    b.Status != SessionStatus.Cancelled)
                .Include(b => b.User)
                .Include(b => b.InteractiveActivity)
                    .ThenInclude(a => a.Track)
                .OrderBy(b => b.BookedAt)
                .Select(b => new
                {
                    id = b.Id,
                    studentId = b.UserId,
                    interactiveActivityId = b.InteractiveActivityId,
                    activityTitle = b.InteractiveActivity.Title,
                    trackTitle = b.InteractiveActivity.Track.Title,
                    requestedAt = b.BookedAt,
                    requestedPrice = b.InteractiveActivity.Price,
                    approvalStatus = b.ApprovalStatus,
                    status = b.Status,
                    date = b.InteractiveActivity.Date,
                    startTime = b.InteractiveActivity.StartTime,
                    endTime = b.InteractiveActivity.EndTime,
                    student = new
                    {
                        fullName = b.User.FullName,
                        userName = b.User.UserName,
                        email = b.User.Email
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }
        // PUT: api/Admin/interactive-bookings/{id}/approve
        [HttpPut("interactive-bookings/{id}/approve")]
        public async Task<IActionResult> ApproveInteractiveBooking(int id)
        {
            var booking = await _context.InteractiveBookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null ||
                booking.Source != BookingSource.Individual)
            {
                return NotFound(new
                {
                    message = "Individual interactive booking request not found."
                });
            }

            if (booking.ApprovalStatus != BookingApprovalStatus.Pending)
            {
                return BadRequest(new
                {
                    message = "This request has already been processed."
                });
            }

            if (booking.Status == SessionStatus.Cancelled)
            {
                return BadRequest(new
                {
                    message = "This booking has been cancelled."
                });
            }

            booking.ApprovalStatus = BookingApprovalStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Individual interactive booking approved successfully."
            });
        }

        // GET: api/Admin/pending-workshop-bookings
        [HttpGet("pending-workshop-bookings")]
        public async Task<IActionResult> GetPendingWorkshopBookings()
        {
            var requests = await _context.WorkshopBookings
                .AsNoTracking()
                .Where(b =>
                    b.Source == BookingSource.Individual &&
                    b.ApprovalStatus == BookingApprovalStatus.Pending &&
                    b.Status != SessionStatus.Cancelled)
                .Include(b => b.User)
                .Include(b => b.Workshop)
                    .ThenInclude(w => w.Track)
                .OrderBy(b => b.BookedAt)
                .Select(b => new
                {
                    id = b.Id,
                    studentId = b.UserId,
                    workshopId = b.WorkshopId,
                    workshopTitle = b.Workshop.Title,
                    trackTitle = b.Workshop.Track.Title,
                    requestedAt = b.BookedAt,
                    requestedPrice = b.Workshop.Price,
                    approvalStatus = b.ApprovalStatus,
                    status = b.Status,
                    date = b.Workshop.Date,
                    startTime = b.Workshop.StartTime,
                    endTime = b.Workshop.EndTime,
                    place = b.Workshop.Place,
                    student = new
                    {
                        fullName = b.User.FullName,
                        userName = b.User.UserName,
                        email = b.User.Email
                    }
                })
                .ToListAsync();

            return Ok(requests);
        }
        // PUT: api/Admin/workshop-bookings/{id}/approve
        [HttpPut("workshop-bookings/{id}/approve")]
        public async Task<IActionResult> ApproveWorkshopBooking(int id)
        {
            var booking = await _context.WorkshopBookings
                .Include(b => b.Workshop)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null ||
                booking.Source != BookingSource.Individual)
            {
                return NotFound(new
                {
                    message = "Individual workshop booking request not found."
                });
            }

            if (booking.ApprovalStatus != BookingApprovalStatus.Pending)
            {
                return BadRequest(new
                {
                    message = "This request has already been processed."
                });
            }

            if (booking.Status == SessionStatus.Cancelled ||
                booking.Workshop.Status == SessionStatus.Cancelled ||
                booking.Workshop.Status == SessionStatus.Finished)
            {
                return BadRequest(new
                {
                    message = "This workshop booking cannot be approved."
                });
            }

            booking.ApprovalStatus = BookingApprovalStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Individual workshop booking approved successfully."
            });
        }// PUT: api/Admin/workshop-bookings/{id}/reject
        [HttpPut("workshop-bookings/{id}/reject")]
        public async Task<IActionResult> RejectWorkshopBooking(int id)
        {
            var booking = await _context.WorkshopBookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null ||
                booking.Source != BookingSource.Individual)
            {
                return NotFound(new
                {
                    message = "Individual workshop booking request not found."
                });
            }

            if (booking.ApprovalStatus != BookingApprovalStatus.Pending)
            {
                return BadRequest(new
                {
                    message = "This request has already been processed."
                });
            }

            booking.ApprovalStatus = BookingApprovalStatus.Rejected;
            booking.Status = SessionStatus.Cancelled;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Individual workshop booking rejected successfully."
            });
        }
        // ---------- Private sessions (admin) ----------
  

        private string CurrentAdminId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private IActionResult PrivateError(Exception ex) => ex switch
        {
            KeyNotFoundException => NotFound(new { message = ex.Message }),
            UnauthorizedAccessException => Forbid(),
            ArgumentException or InvalidOperationException => BadRequest(new { message = ex.Message }),
            _ => StatusCode(500, new { message = "An unexpected error occurred." })
        };

        // POST: api/Admin/private/availability

        // POST: api/Admin/private/availability
        [HttpPost("private/availability")]
        public async Task<IActionResult> CreateInstructorAvailability(
            [FromBody] CreatePrivateAvailabilityDto request)
        {
            try
            {
                var availability = await _privateSessions.CreateAvailabilityAsync(
                    CurrentAdminId,
                    request.InstructorId,
                    request.DayOfWeek,
                    request.StartTime,
                    request.EndTime,
                    request.ValidFrom,
                    request.ValidTo);

                return Ok(new PrivateAvailabilityDto
                {
                    Id = availability.Id,
                    InstructorId = availability.InstructorId,
                    InstructorName = availability.Instructor?.FullName ?? "",
                    DayOfWeek = availability.DayOfWeek,
                    StartTime = availability.StartTime,
                    EndTime = availability.EndTime,
                    ValidFrom = availability.ValidFrom,
                    ValidTo = availability.ValidTo,
                    IsActive = availability.IsActive
                });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // GET: api/Admin/private/availability/{instructorId}
        [HttpGet("private/availability/{instructorId}")]
        public async Task<IActionResult> GetInstructorAvailability(string instructorId)
        {
            try
            {
                var slots = await _privateSessions.GetInstructorAvailabilityAsync(instructorId);

                return Ok(slots.Select(x => new PrivateAvailabilityDto
                {
                    Id = x.Id,
                    InstructorId = x.InstructorId,
                    InstructorName = x.Instructor?.FullName ?? "",
                    DayOfWeek = x.DayOfWeek,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    ValidFrom = x.ValidFrom,
                    ValidTo = x.ValidTo,
                    IsActive = x.IsActive
                }));
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // DELETE: api/Admin/private/availability/{id}
        [HttpDelete("private/availability/{id:int}")]
        public async Task<IActionResult> DeactivateInstructorAvailability(int id)
        {
            try
            {
                await _privateSessions.DeactivateAvailabilityAsync(CurrentAdminId, id);
                return Ok(new { message = "Availability slot deactivated." });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // POST: api/Admin/private/arrangements
        [HttpPost("private/arrangements")]
        public async Task<IActionResult> CreatePrivateArrangement(
            [FromBody] CreatePrivateArrangementDto request)
        {
            try
            {
                var arrangement = await _privateSessions.CreateArrangementAsync(
                   CurrentAdminId,
                   request.StudentId,
                   request.InstructorId,
                   request.TrackId,
                   request.MeetingLink,
                   request.DayOfWeek,
                   request.StartTime,
                   request.EndTime,
                   request.StartDate,
                   request.EndDate);

                return Ok(new PrivateArrangementDto
                {
                    Id = arrangement.Id,
                    StudentId = arrangement.StudentId,
                    StudentName = arrangement.Student?.FullName ?? "",
                    InstructorId = arrangement.InstructorId,
                    InstructorName = arrangement.Instructor?.FullName ?? "",
                    TrackId = arrangement.TrackId,
                    TrackTitle = arrangement.Track?.Title ?? "",
                    DayOfWeek = arrangement.DayOfWeek,
                    StartTime = arrangement.StartTime,
                    EndTime = arrangement.EndTime,
                    StartDate = arrangement.StartDate,
                    EndDate = arrangement.EndDate,
                    MeetingLink = arrangement.MeetingLink,
                    IsActive = arrangement.IsActive,
                    ApprovalStatus = arrangement.ApprovalStatus
                });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // POST: api/Admin/private/arrangements/{id}/generate?weeksAhead=12
        [HttpPost("private/arrangements/{id:int}/generate")]
        public async Task<IActionResult> GeneratePrivateOccurrences(
            int id,
            [FromQuery] int weeksAhead = 12)
        {
            try
            {
                var created = await _privateSessions.GenerateOccurrencesAsync(id, weeksAhead);
                return Ok(new { created });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // POST: api/Admin/private/occurrences/{id}/replacement
        [HttpPost("private/occurrences/{id:int}/replacement")]
        public async Task<IActionResult> AssignPrivateReplacement(
            int id,
            [FromBody] AssignPrivateReplacementDto request)
        {
            try
            {
                var replacement = await _privateSessions.AssignReplacementAsync(
                    CurrentAdminId,
                    request.MeetingLink,
                    id,
                    request.StartUtc,
                    request.EndUtc);

                return Ok(new
                {
                    replacement.Id,
                    replacement.MissedOccurrenceId,
                    replacement.ReplacementOccurrenceId,
                    replacement.StudentId,
                    replacement.AssignedByAdminId,
                    replacement.AssignedAtUtc
                });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // GET: api/Admin/private/requests
        [HttpGet("private/requests")]
        public async Task<IActionResult> GetPrivateRequests(
            [FromQuery] BookingApprovalStatus? status = null,
            [FromQuery] RequestType? requestType = null)
        {
            try
            {
                var requests = await _privateSessions.GetAdminRequestsAsync(status, requestType);

                return Ok(requests.Select(r => new PrivateRequestDto
                {
                    Id = r.Id,
                    StudentId = r.StudentId,
                    StudentName = r.Occurrence.Student?.FullName ?? "",
                    OccurrenceId = r.OccurrenceId,
                    InstructorName = r.Occurrence.Instructor?.FullName ?? "",
                    TrackTitle = r.Occurrence.Track?.Title ?? "",
                    StartUtc = r.Occurrence.StartUtc,
                    EndUtc = r.Occurrence.EndUtc,
                    RequestType = r.RequestType,
                    Status = r.Status,
                    CreatedAtUtc = r.CreatedAtUtc
                }));
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // PUT: api/Admin/private/requests/{requestId}/review
        [HttpPut("private/requests/{requestId:int}/review")]
        public async Task<IActionResult> ReviewPrivateRequest(
            int requestId,
            [FromBody] ReviewPrivateRequestDto request)
        {
            try
            {
                await _privateSessions.ReviewPrivateRequestAsync(
                    CurrentAdminId,
                    requestId,
                    request.Decision);

                return Ok(new
                {
                    message = request.Decision == BookingApprovalStatus.Approved
                        ? "Private session request approved successfully."
                        : "Private session request rejected successfully."
                });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // GET: api/Admin/private/arrangements
        [HttpGet("private/arrangements")]
        public async Task<IActionResult> GetAllPrivateArrangements()
        {
            try
            {
                var arrangements = await _privateSessions.GetAllArrangementsAsync();

                return Ok(arrangements.Select(a => new PrivateArrangementDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student?.FullName ?? "",
                    InstructorId = a.InstructorId,
                    InstructorName = a.Instructor?.FullName ?? "",
                    TrackId = a.TrackId,
                    TrackTitle = a.Track?.Title ?? "",
                    DayOfWeek = a.DayOfWeek,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    MeetingLink = a.MeetingLink,
                    IsActive = a.IsActive,
                    ApprovalStatus = a.ApprovalStatus
                }));
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // GET: api/Admin/private/occurrences
        [HttpGet("private/occurrences")]
        public async Task<IActionResult> GetPrivateOccurrences(
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] string? studentId,
            [FromQuery] string? instructorId,
            [FromQuery] SessionStatus? status)
        {
            try
            {
                var from = fromUtc ?? DateTime.UtcNow;
                var to = toUtc ?? DateTime.UtcNow.AddDays(84);

                if (to <= from)
                    return BadRequest(new { message = "The end date must be after the start date." });

                var occurrences = await _privateSessions.GetAdminOccurrencesAsync(
                    from, to, studentId, instructorId, status);

                return Ok(occurrences.Select(o => new AdminPrivateOccurrenceDto
                {
                    Id = o.Id,
                    PrivateArrangementId = o.PrivateArrangementId,
                    StudentId = o.StudentId,
                    StudentName = o.Student?.FullName ?? "",
                    InstructorId = o.InstructorId,
                    InstructorName = o.Instructor?.FullName ?? "",
                    TrackId = o.TrackId,
                    TrackTitle = o.Track?.Title ?? "",
                    StartUtc = o.StartUtc,
                    EndUtc = o.EndUtc,
                    Status = o.Status,
                    IsReplacement = o.Replacement != null
                }));
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // PUT: api/Admin/private/arrangements/{id}/end
        [HttpPut("private/arrangements/{id:int}/end")]
        public async Task<IActionResult> EndPrivateArrangement(int id)
        {
            try
            {
                await _privateSessions.EndArrangementAsync(CurrentAdminId, id);
                return Ok(new { message = "Private arrangement ended successfully." });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

        // PUT: api/Admin/private/occurrences/{id}/status
        [HttpPut("private/occurrences/{id:int}/status")]
        public async Task<IActionResult> UpdatePrivateOccurrenceStatus(
            int id,
            [FromBody] UpdatePrivateOccurrenceStatusDto request)
        {
            try
            {
                await _privateSessions.UpdateOccurrenceStatusAsync(
                    CurrentAdminId, id, request.Status);

                return Ok(new { message = "Private occurrence status updated successfully." });
            }
            catch (Exception ex) { return PrivateError(ex); }
        }

    }
}
