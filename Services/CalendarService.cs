using Engineering_Hub.DTO.Calendar;
using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class CalendarService
    {
        private readonly EngineeringHubContext _context;

        public CalendarService(EngineeringHubContext context)
        {
            _context = context;
        }

        public async Task<List<CalendarEventDto>> GetInstructorCalendarAsync(
    string instructorId)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var currentTime = now.TimeOfDay;

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
                    Status = a.Status
                })
                .ToListAsync();

            return workshops
                .Concat(activities)
                .OrderBy(e => e.StartDateTime)
                .ToList();
        }


        public async Task<List<CalendarEventDto>> GetStudentCalendarAsync(
            string studentId)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var currentTime = now.TimeOfDay;

            // Get the student's upcoming workshop bookings.
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

            // Get the student's upcoming interactive activity bookings.
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
                    Status = a.Status
                })
                .ToList();

            return workshops
                .Concat(activities)
                .OrderBy(e => e.StartDateTime)
                .ToList();
        }

    }
}