
using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly ITrackBookingService _trackBookingService;
        private readonly EngineeringHubContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ITrackBookingService trackBookingService,
            EngineeringHubContext context,
            UserManager<ApplicationUser> userManager)
        {
            _trackBookingService = trackBookingService;
            _context = context;
            _userManager = userManager;
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
    }
}
