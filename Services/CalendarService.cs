
using Engineering_Hub.DTO.Calendar;
using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class CalendarService
    {
        private readonly EngineeringHubContext _context;
        private readonly IConfiguration _config;

        private TimeZoneInfo AdminZone =>
            TimeZoneInfo.FindSystemTimeZoneById(
                _config["PrivateSessions:AdminTimeZoneId"]
                ?? throw new InvalidOperationException(
                    "PrivateSessions:AdminTimeZoneId is not configured."));

        public CalendarService(
            EngineeringHubContext context,
            IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<List<CalendarEventDto>> GetInstructorCalendarAsync(
            string instructorId)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var currentTime = now.TimeOfDay;

            // Existing Workshop events.
            var workshops = await _context.Workshops
                .AsNoTracking()
                .Where(w =>
                    w.InstructorId == instructorId &&
                    w.Status != SessionStatus.Cancelled &&
                    (w.Date.Date > today ||
                     (w.Date.Date == today && w.EndTime > currentTime)))
                .Select(w => new CalendarEventDto
                {
                    Id = w.Id,
                    Title = w.Title,
                    EventType = "Workshop",
                    TrackId = w.TrackId,
                    TrackTitle = w.Track.Title,
                    StartDateTime = w.Date.Date.Add(w.StartTime),
                    EndDateTime = w.Date.Date.Add(w.EndTime),
                    Status = w.Status
                })
                .ToListAsync();

            // Existing Interactive Activity events.
            var activities = await _context.InteractiveActivities
                .AsNoTracking()
                .Where(a =>
                    a.InstructorId == instructorId &&
                    a.Status != SessionStatus.Cancelled &&
                    (a.Date.Date > today ||
                     (a.Date.Date == today && a.EndTime > currentTime)))
                .Select(a => new CalendarEventDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    EventType = "Interactive",
                    TrackId = a.TrackId,
                    TrackTitle = a.Track.Title,
                    StartDateTime = a.Date.Date.Add(a.StartTime),
                    EndDateTime = a.Date.Date.Add(a.EndTime),
                    Status = a.Status,
                    MeetingUrl = a.MeetingUrl
                })
                .ToListAsync();

            // Private calendar display horizon: 365 days.
            var zone = AdminZone;
            var localToday = TimeZoneInfo.ConvertTimeFromUtc(now, zone).Date;
            var horizonEnd = localToday.AddDays(365);

            // Get the instructor's active availability records.
            var availabilityRecords = await _context.PrivateAvailabilities
                .AsNoTracking()
                .Where(a =>
                    a.InstructorId == instructorId &&
                    a.IsActive)
                .ToListAsync();

            var availabilityEvents = new List<CalendarEventDto>();

            foreach (var availability in availabilityRecords)
            {
                for (var day = localToday; day < horizonEnd; day = day.AddDays(1))
                {
                    if (day.DayOfWeek != availability.DayOfWeek)
                        continue;

                    if (day < availability.ValidFrom.Date)
                        continue;

                    if (availability.ValidTo.HasValue &&
                        day > availability.ValidTo.Value.Date)
                        continue;

                    availabilityEvents.Add(new CalendarEventDto
                    {
                        // Negative IDs distinguish generated availability events.
                        Id = -((availability.Id * 1000) +
                               (day - localToday).Days + 1),

                        Title = "Available",
                        EventType = "PrivateAvailability",
                        TrackId = 0,
                        TrackTitle = string.Empty,
                        StartDateTime = day.Add(availability.StartTime),
                        EndDateTime = day.Add(availability.EndTime),
                        Status = SessionStatus.Scheduled
                    });
                }
            }

            // Convert the local calendar horizon to UTC for occurrence querying.
            var horizonUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(horizonEnd, DateTimeKind.Unspecified),
                zone);

            // Get the instructor's upcoming private occurrences.
            // Cancelled occurrences are included so the released time remains visible.
            var privateOccurrences = await _context.PrivateOccurrences
                .AsNoTracking()
                .Where(o =>
                    o.InstructorId == instructorId &&
                    o.EndUtc > now &&
                    o.StartUtc < horizonUtc)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.StartUtc,
                    o.EndUtc,
                    StudentName = o.Student.FullName,
                    o.TrackId,
                    TrackTitle = o.Track.Title,
                    MeetingUrl = o.MeetingLink
                })
                .ToListAsync();

            var privateSessionEvents = privateOccurrences
                .Select(o =>
                {
                    var isCancelled = o.Status == SessionStatus.Cancelled;

                    return new CalendarEventDto
                    {
                        Id = o.Id,
                        Title = isCancelled ? "Available" : "Private Session",
                        EventType = "PrivateSession",
                        TrackId = o.TrackId,
                        TrackTitle = o.TrackTitle,

                        // Private occurrences are stored in UTC.
                        // Convert them to the admin's local time for the calendar.
                        StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.SpecifyKind(o.StartUtc, DateTimeKind.Utc),
                            zone),

                        EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.SpecifyKind(o.EndUtc, DateTimeKind.Utc),
                            zone),

                        Status = o.Status,
                        StudentName = isCancelled ? null : o.StudentName,
                        MeetingUrl = isCancelled ? null : o.MeetingUrl
                    };
                })
                .ToList();

            return workshops
                .Concat(activities)
                .Concat(availabilityEvents)
                .Concat(privateSessionEvents)
                .OrderBy(e => e.StartDateTime)
                .ToList();
        }

        public async Task<List<CalendarEventDto>> GetStudentCalendarAsync(
            string studentId)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var currentTime = now.TimeOfDay;

            // Identify the student's account type.
            var accountType = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == studentId)
                .Select(u => (AccountType?)u.AccountType)
                .FirstOrDefaultAsync();

            if (accountType == null)
                throw new KeyNotFoundException("Student not found.");

            // Get the student's upcoming Workshop bookings.
            // This applies to both GroupStudent and PrivateStudent.
            var workshopBookings = await _context.WorkshopBookings
                .AsNoTracking()
                .Where(b =>
                    b.UserId == studentId &&
                    b.Status != SessionStatus.Cancelled &&
                    b.ApprovalStatus != BookingApprovalStatus.Rejected &&
                    b.Workshop.Status != SessionStatus.Cancelled &&
                    (
                        b.Workshop.Date.Date > today ||
                        (b.Workshop.Date.Date == today &&
                         b.Workshop.EndTime > currentTime)
                    ))
                .Select(b => new
                {
                    WorkshopId = b.Workshop.Id,
                    b.Workshop.Title,
                    b.Workshop.TrackId,
                    TrackTitle = b.Workshop.Track.Title,
                    b.Workshop.Date,
                    b.Workshop.StartTime,
                    b.Workshop.EndTime,
                    Status = b.Workshop.Status
                })
                .ToListAsync();

            var workshops = workshopBookings
                .Select(w => new CalendarEventDto
                {
                    Id = w.WorkshopId,
                    Title = w.Title,
                    EventType = "Workshop",
                    TrackId = w.TrackId,
                    TrackTitle = w.TrackTitle,
                    StartDateTime = w.Date.Date.Add(w.StartTime),
                    EndDateTime = w.Date.Date.Add(w.EndTime),
                    Status = w.Status
                })
                .ToList();

            // Private students see their Private Sessions, not Interactive Activities.
            if (accountType == AccountType.PrivateStudent)
            {
                var privateOccurrences = await _context.PrivateOccurrences
                    .AsNoTracking()
                    .Where(o =>
                        o.StudentId == studentId &&
                        o.EndUtc > now)
                    .Select(o => new
                    {
                        o.Id,
                        o.Status,
                        o.StartUtc,
                        o.EndUtc,
                        o.TrackId,
                        TrackTitle = o.Track.Title,
                        MeetingUrl = o.MeetingLink
                    })
                    .ToListAsync();

                var zone = AdminZone;

                var privateSessions = privateOccurrences
                    .Select(o => new CalendarEventDto
                    {
                        Id = o.Id,
                        Title = o.Status == SessionStatus.Cancelled
                            ? "Cancelled Private Session"
                            : "Private Session",
                        EventType = "PrivateSession",
                        TrackId = o.TrackId,
                        TrackTitle = o.TrackTitle,
                        StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.SpecifyKind(o.StartUtc, DateTimeKind.Utc),
                            zone),
                        EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.SpecifyKind(o.EndUtc, DateTimeKind.Utc),
                            zone),
                        Status = o.Status,
                        MeetingUrl = o.Status == SessionStatus.Cancelled
                            ? null
                            : o.MeetingUrl
                    })
                    .ToList();

                return workshops
                    .Concat(privateSessions)
                    .OrderBy(e => e.StartDateTime)
                    .ToList();
            }

            // Group students retain the existing Interactive Activity bookings.
            var activityBookings = await _context.InteractiveBookings
                .AsNoTracking()
                .Where(b =>
                    b.UserId == studentId &&
                    b.Status != SessionStatus.Cancelled &&
                    b.InteractiveActivity.Status != SessionStatus.Cancelled &&
                    (
                        b.InteractiveActivity.Date.Date > today ||
                        (b.InteractiveActivity.Date.Date == today &&
                         b.InteractiveActivity.EndTime > currentTime)
                    ))
                .Select(b => new
                {
                    ActivityId = b.InteractiveActivity.Id,
                    b.InteractiveActivity.Title,
                    b.InteractiveActivity.TrackId,
                    TrackTitle = b.InteractiveActivity.Track.Title,
                    b.InteractiveActivity.Date,
                    b.InteractiveActivity.StartTime,
                    b.InteractiveActivity.EndTime,
                    b.InteractiveActivity.MeetingUrl,
                    Status = b.InteractiveActivity.Status
                })
                .ToListAsync();

            var activities = activityBookings
                .Select(a => new CalendarEventDto
                {
                    Id = a.ActivityId,
                    Title = a.Title,
                    EventType = "Interactive",
                    TrackId = a.TrackId,
                    TrackTitle = a.TrackTitle,
                    StartDateTime = a.Date.Date.Add(a.StartTime),
                    EndDateTime = a.Date.Date.Add(a.EndTime),
                    Status = a.Status,
                    MeetingUrl = a.MeetingUrl
                })
                .ToList();

            return workshops
                .Concat(activities)
                .OrderBy(e => e.StartDateTime)
                .ToList();
        }
    }
}
