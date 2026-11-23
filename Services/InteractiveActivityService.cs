using AutoMapper;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.InteractiveActivity;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Engineering_Hub.Services
{
    public class InteractiveActivityService : IInteractiveActivityService
    {
        private readonly UnitWork _unitWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;
        public InteractiveActivityService(
         UnitWork unitWork,
         IMapper mapper,
         IWebHostEnvironment environment,
         IConfiguration config)
        {
            _unitWork = unitWork;
            _mapper = mapper;
            _environment = environment;
            _config = config;
        }

        private TimeZoneInfo AdminTimeZone =>
    TimeZoneInfo.FindSystemTimeZoneById(
        _config["PrivateSessions:AdminTimeZoneId"]
        ?? throw new InvalidOperationException(
            "PrivateSessions:AdminTimeZoneId is not configured."));

        private DateTime ConvertEgyptTimeToUtc(DateTime date, TimeSpan time)
        {
            var localDateTime = DateTime.SpecifyKind(
                date.Date.Add(time),
                DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(
                localDateTime,
                AdminTimeZone);
        }


        private string? CheckInstructorScheduleConflict(
            string instructorId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludedActivityId = null)
        {
            var newStartUtc = ConvertEgyptTimeToUtc(date, startTime);
            var newEndUtc = ConvertEgyptTimeToUtc(date, endTime);

            // 1. Check other Interactive Activities.
            var activities = _unitWork.InteractiveActivityrepo
                .GetByCondition(x =>
                    x.InstructorId == instructorId &&
                    x.Status != SessionStatus.Cancelled &&
                    (x.ApprovalStatus == ActivityApprovalStatus.Pending ||
                     x.ApprovalStatus == ActivityApprovalStatus.Approved) &&
                    (!excludedActivityId.HasValue ||
                     x.Id != excludedActivityId.Value))
                .ToList();

            foreach (var activity in activities)
            {
                var existingStartUtc = ConvertEgyptTimeToUtc(
                    activity.Date, activity.StartTime);

                var existingEndUtc = ConvertEgyptTimeToUtc(
                    activity.Date, activity.EndTime);

                if (existingStartUtc < newEndUtc &&
                    existingEndUtc > newStartUtc)
                {
                    return "The instructor already has an interactive activity during this time.";
                }
            }

            // 2. Check Workshops.
            var workshops = _unitWork.Workshoprepo
                .GetByCondition(x =>
                    x.InstructorId == instructorId &&
                    x.Status != SessionStatus.Cancelled &&
                    (x.ApprovalStatus == ActivityApprovalStatus.Pending ||
                     x.ApprovalStatus == ActivityApprovalStatus.Approved))
                .ToList();

            foreach (var workshop in workshops)
            {
                var existingStartUtc = ConvertEgyptTimeToUtc(
                    workshop.Date, workshop.StartTime);

                var existingEndUtc = ConvertEgyptTimeToUtc(
                    workshop.Date, workshop.EndTime);

                if (existingStartUtc < newEndUtc &&
                    existingEndUtc > newStartUtc)
                {
                    return "The instructor already has a workshop during this time.";
                }
            }

            // 3. Check Private Session occurrences.
            var privateOccurrences = _unitWork.PrivateOccurrencerepo
                .GetByCondition(x =>
                    x.InstructorId == instructorId &&
                    x.Status != SessionStatus.Cancelled)
                .ToList();

            foreach (var occurrence in privateOccurrences)
            {
                if (occurrence.StartUtc < newEndUtc &&
                    occurrence.EndUtc > newStartUtc)
                {
                    return "The instructor already has a private session during this time.";
                }
            }

            return null;
        }

        // CREATE
        public async Task<InteractiveActivityResponseDTO?> CreateActivityAsync(
            InteractiveActivityDTO dto,
            string instructorId)
        {
            // 1. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return null;

            // 2. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return null;

            // 3. Validate price
            if (dto.Price < 0)
                return null;

            // 4. Validate capacity
            if (dto.Capacity <= 0)
                return null;

            // 5. Validate time
            if (dto.EndTime <= dto.StartTime)
                return null;

            // 6. Validate date
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return null;

            var conflict = CheckInstructorScheduleConflict(
    instructorId,
    dto.Date,
    dto.StartTime,
    dto.EndTime);

            if (conflict != null)
            {
                return null;
            }
            var activity = _mapper.Map<InteractiveActivity>(dto);
            activity.InstructorId = instructorId;
            activity.Status = SessionStatus.Scheduled;

            activity.ApprovalStatus = ActivityApprovalStatus.Pending;
            // 9. Add
            _unitWork.InteractiveActivityrepo.add(activity);

            // 10. Save
            await _unitWork.SaveAsync();

            // 11. Map Entity → Response DTO
            return _mapper.Map<InteractiveActivityResponseDTO>(
                activity);
        }


        // GET BY TRACK
        public async Task<List<InteractiveActivityResponseDTO>>
            GetActivitiesByTrackAsync(int trackId)
        {
            var activities = _unitWork.InteractiveActivityrepo
                  .GetByCondition(x =>
                      x.TrackId == trackId &&
                      x.Status != SessionStatus.Cancelled &&
                      x.ApprovalStatus == ActivityApprovalStatus.Approved)
                  .ToList();

            var result = _mapper.Map<List<InteractiveActivityResponseDTO>>(
                activities);

            foreach (var activity in result)
            {
                var currentBookings = _unitWork.InteractiveBookingrepo
                    .GetByCondition(b =>
                        b.InteractiveActivityId == activity.Id &&
                        b.Status != SessionStatus.Cancelled &&
                        b.ApprovalStatus != BookingApprovalStatus.Rejected)
                    .Count();

                activity.AvailableCapacity = Math.Max(
                    0,
                    activity.Capacity - currentBookings);
            }

            return result;
        }


        // UPDATE
        public async Task<(bool Success, string Message)>
            UpdateActivityAsync(
                int id,
                InteractiveActivityDTO dto,
                string instructorId)
        {
            // 1. Find activity
            var activity = _unitWork.InteractiveActivityrepo
                .GetById(id);

            if (activity == null)
                return (false, "Interactive activity not found.");

            // 2. Don't update cancelled activity
            if (activity.Status == SessionStatus.Cancelled)
                return (false,
                    "Cancelled activity cannot be updated.");
            if (activity.InstructorId != instructorId)
                return (false, "You are not authorized to edit this activity.");

            if (dto.TrackId != activity.TrackId)
                return (false, "An activity's track cannot be changed.");
            // 3. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return (false,
                    "Track not found or inactive.");

            // 4. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false,
                    "You are not assigned to this Track.");

            // 5. Validate price
            if (dto.Price < 0)
                return (false,
                    "Price cannot be negative.");

            // 6. Validate capacity
            if (dto.Capacity <= 0)
                return (false,
                    "Capacity must be greater than zero.");

            // 7. Validate time
            if (dto.EndTime <= dto.StartTime)
                return (false,
                    "End time must be after start time.");

            // 8. Validate date
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return (false,
                    "Activity date cannot be in the past.");
            // 9. Check instructor schedule conflicts
            var conflict = CheckInstructorScheduleConflict(
                instructorId,
                dto.Date,
                dto.StartTime,
                dto.EndTime,
                activity.Id);

            if (conflict != null)
                return (false, conflict);
            // 9. Map DTO → existing entity
            _mapper.Map(dto, activity);
            // Instructor edits require Admin approval again.
            activity.ApprovalStatus = ActivityApprovalStatus.Pending;

            // Status remains unchanged

            // 10. Save
            await _unitWork.SaveAsync();

            return (true,
                "Interactive activity updated successfully.");
        }


        // CANCEL
        public async Task<(bool Success, string Message)>
            CancelActivityAsync(
                int id,
                string instructorId)
        {
            // 1. Find activity
            var activity = _unitWork.InteractiveActivityrepo
                .GetById(id);

            if (activity == null)
                return (false,
                    "Interactive activity not found.");
            if (activity.InstructorId != instructorId)
                return (false, "You are not authorized to cancel this activity.");
            // 2. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == activity.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false,
                    "You are not assigned to this Track.");

            // 3. Check already cancelled
            if (activity.Status == SessionStatus.Cancelled)
                return (false,
                    "Interactive activity is already cancelled.");

            // 4. Cancel
            activity.Status = SessionStatus.Cancelled;

            // 5. Save
            await _unitWork.SaveAsync();

            return (true,
                "Interactive activity cancelled successfully.");
        }


        // CREATE A RECURRING SCHEDULE
        public async Task<InteractiveSchedule?> CreateScheduleAsync(
            InteractiveScheduleDTO dto,
            string instructorId)
        {
            if (dto.DaysOfWeek == null ||
                string.IsNullOrWhiteSpace(dto.Title))
                return null;

            var days = dto.DaysOfWeek.Distinct().ToList();

            if (days.Count == 0 ||
     days.Any(day => !Enum.IsDefined(typeof(DayOfWeek), day)) ||
     dto.StartDate.Date < DateTime.UtcNow.Date ||
     dto.EndDate.Date < dto.StartDate.Date ||
     dto.EndTime <= dto.StartTime ||
     dto.Price < 0 ||
     dto.Capacity <= 0)
            {
                return null;
            }

            var futureDates = new List<DateTime>();
            var nowUtc = DateTime.UtcNow;

            for (var date = dto.StartDate.Date;
                 date <= dto.EndDate.Date;
                 date = date.AddDays(1))
            {
                if (!days.Contains(date.DayOfWeek))
                    continue;

                var occurrenceStartUtc = ConvertEgyptTimeToUtc(
                    date,
                    dto.StartTime);

                if (occurrenceStartUtc > nowUtc)
                    futureDates.Add(date);
            }

            if (futureDates.Count == 0)
                return null;

            var track = _unitWork.Trackrepo.GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return null;

            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return null;
            // Check every future occurrence before creating the schedule.
            foreach (var date in futureDates)
            {
                var conflict = CheckInstructorScheduleConflict(
                    instructorId,
                    date,
                    dto.StartTime,
                    dto.EndTime);

                if (conflict != null)
                    return null;
            }
            var schedule = new InteractiveSchedule
            {
                TrackId = dto.TrackId,
                InstructorId = instructorId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                Price = dto.Price,
                Capacity = dto.Capacity,
                MeetingUrl = dto.MeetingUrl,
                DaysOfWeek = string.Join(",", days.Select(day => (int)day)),
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsActive = true
            };

            _unitWork.InteractiveScheduleRepo.add(schedule);

            // Save to generate the schedule ID.
            await _unitWork.SaveAsync();

            // Generate an activity for each selected weekday.
            foreach (var date in futureDates)
            {
                if (!days.Contains(date.DayOfWeek))
                    continue;

                var activity = new InteractiveActivity
                {
                    TrackId = schedule.TrackId,
                    InstructorId = instructorId,
                    Title = schedule.Title,
                    Description = schedule.Description,
                    Price = schedule.Price,
                    Capacity = schedule.Capacity,
                    MeetingUrl = schedule.MeetingUrl,
                    Date = date,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    Status = SessionStatus.Scheduled,
                    ApprovalStatus = ActivityApprovalStatus.Pending,
                    InteractiveScheduleId = schedule.Id
                };

                _unitWork.InteractiveActivityrepo.add(activity);
            }

            await _unitWork.SaveAsync();

            return schedule;
        }


        // GET RECURRING SCHEDULES BY TRACK
        public Task<List<InteractiveSchedule>> GetSchedulesByTrackAsync(
            int trackId)
        {
            var schedules = _unitWork.InteractiveScheduleRepo
                .GetByCondition(x => x.TrackId == trackId);

            return Task.FromResult(schedules);
        }


        // UPDATE AN ENTIRE RECURRING SCHEDULE
        public async Task<(bool Success, string Message)> UpdateScheduleAsync(
            int scheduleId,
            InteractiveScheduleDTO dto,
            string instructorId)
        {
            var schedule = _unitWork.InteractiveScheduleRepo
                .GetById(scheduleId);

            if (schedule == null)
                return (false, "Recurring schedule not found.");

            if (schedule.InstructorId != instructorId)
                return (false, "You are not authorized to edit this schedule.");

            if (!schedule.IsActive)
                return (false, "This schedule has already ended.");

            if (dto.TrackId != schedule.TrackId)
                return (false, "A schedule's track cannot be changed.");

            var days = dto.DaysOfWeek?
                .Distinct()
                .ToList();

            var today = DateTime.UtcNow.Date;
            var now = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(dto.Title) ||
                days == null ||
                days.Count == 0 ||
                days.Any(day =>
                    !Enum.IsDefined(typeof(DayOfWeek), day)) ||
                dto.EndDate.Date < today ||
                dto.EndDate.Date < dto.StartDate.Date ||
                dto.EndTime <= dto.StartTime ||
                dto.Price < 0 ||
                dto.Capacity <= 0)
            {
                return (false, "The schedule data is invalid.");
            }

            var instructorAssignment = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == schedule.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructorAssignment == null)
                return (false, "You are no longer assigned to this track.");

            var desiredDates = new HashSet<DateTime>();

            for (var date = dto.StartDate.Date;
                 date <= dto.EndDate.Date;
                 date = date.AddDays(1))
            {
                if (days.Contains(date.DayOfWeek) &&
      ConvertEgyptTimeToUtc(date, dto.StartTime) > now)
                {
                    desiredDates.Add(date);
                }
            }

            if (desiredDates.Count == 0)
                return (false,
                    "The selected date range contains no future sessions.");
            // Retrieve existing occurrences before making any changes.
            var existingActivities = _unitWork.InteractiveActivityrepo
                .GetByCondition(a =>
                    a.InteractiveScheduleId == scheduleId)
                .ToList();

            // Validate ALL future occurrences before modifying the schedule.
            foreach (var date in desiredDates)
            {
                var existingOccurrence = existingActivities
                    .FirstOrDefault(a =>
                        a.Date.Date == date &&
                        a.Status != SessionStatus.Cancelled &&
                        a.Status != SessionStatus.Finished &&
                        a.Status != SessionStatus.Completed);

                int? excludedActivityId = null;

                if (existingOccurrence != null)
                {
                    var hasBookings = _unitWork.InteractiveBookingrepo
                        .GetByCondition(b =>
                            b.InteractiveActivityId == existingOccurrence.Id &&
                            b.Status != SessionStatus.Cancelled &&
                            b.ApprovalStatus != BookingApprovalStatus.Rejected)
                        .Any();

                    var timeChanged =
                        existingOccurrence.StartTime != dto.StartTime ||
                        existingOccurrence.EndTime != dto.EndTime;

                    // Exclude the existing occurrence only if it will be
                    // updated in place.
                    //
                    // If it has bookings and its time changes, it must remain
                    // in the conflict check because it will be preserved
                    // as an independent session.
                    if (!(hasBookings && timeChanged))
                    {
                        excludedActivityId = existingOccurrence.Id;
                    }
                }

                var conflict = CheckInstructorScheduleConflict(
                    instructorId,
                    date,
                    dto.StartTime,
                    dto.EndTime,
                    excludedActivityId);

                if (conflict != null)
                {
                    return (false,
                        $"Schedule update rejected. {conflict}");
                }
            }
            // Update the recurring schedule itself.
            schedule.Title = dto.Title.Trim();
            schedule.Description = dto.Description;
            schedule.Price = dto.Price;
            schedule.Capacity = dto.Capacity;
            schedule.MeetingUrl = dto.MeetingUrl;
            schedule.DaysOfWeek = string.Join(
                ",", days.Select(day => (int)day));
            schedule.StartDate = dto.StartDate.Date;
            schedule.EndDate = dto.EndDate.Date;
            schedule.StartTime = dto.StartTime;
            schedule.EndTime = dto.EndTime;

            //var existingActivities = _unitWork.InteractiveActivityrepo
            //    .GetByCondition(a =>
            //        a.InteractiveScheduleId == scheduleId);

            // Update or cancel existing future occurrences.
            foreach (var activity in existingActivities)
            {
                var occurrenceDate = activity.Date.Date;
                var occurrenceStart = ConvertEgyptTimeToUtc(
    occurrenceDate,
    activity.StartTime);

                // Never change past or already completed occurrences.
                if (occurrenceStart <= now ||
                    activity.Status == SessionStatus.Cancelled ||
                    activity.Status == SessionStatus.Finished ||
                    activity.Status == SessionStatus.Completed)
                {
                    continue;
                }

                var bookings = _unitWork.InteractiveBookingrepo
                    .GetByCondition(b =>
                        b.InteractiveActivityId == activity.Id &&
                        b.Status != SessionStatus.Cancelled &&
                        b.ApprovalStatus != BookingApprovalStatus.Rejected);

                var hasBookings = bookings.Count > 0;

                var dateRemoved = !desiredDates.Contains(occurrenceDate);
                var timeChanged = activity.StartTime != dto.StartTime ||
                                  activity.EndTime != dto.EndTime;

                // Preserve booked occurrences as one-off sessions if their
                // date is removed from the pattern or their time changes.
                if (hasBookings)
                {
                    if (dateRemoved || timeChanged)
                    {
                        activity.InteractiveScheduleId = null;
                    }

                    continue;
                }

                // Cancel unbooked occurrences that no longer match the pattern.
                if (dateRemoved)
                {
                    activity.Status = SessionStatus.Cancelled;
                    continue;
                }

                // Update unbooked occurrences that remain in the pattern.
                activity.Title = schedule.Title;
                activity.Description = schedule.Description;
                activity.Price = schedule.Price;
                activity.Capacity = schedule.Capacity;
                activity.MeetingUrl = schedule.MeetingUrl;
                activity.StartTime = schedule.StartTime;
                activity.EndTime = schedule.EndTime;
                activity.ApprovalStatus = ActivityApprovalStatus.Pending;
            }

            // Create occurrences for newly selected future dates.
            foreach (var date in desiredDates)
            {
                var alreadyExists = existingActivities.Any(
                     a => a.InteractiveScheduleId == scheduleId &&
                          a.Date.Date == date &&
                          a.Status != SessionStatus.Cancelled &&
                          a.Status != SessionStatus.Finished &&
                          a.Status != SessionStatus.Completed);

                if (alreadyExists)
                    continue;

                var activity = new InteractiveActivity
                {
                    TrackId = schedule.TrackId,
                    InstructorId = instructorId,
                    InteractiveScheduleId = schedule.Id,
                    Title = schedule.Title,
                    Description = schedule.Description,
                    Price = schedule.Price,
                    Capacity = schedule.Capacity,
                    MeetingUrl = schedule.MeetingUrl,
                    Date = date,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    Status = SessionStatus.Scheduled,
                    ApprovalStatus = ActivityApprovalStatus.Pending
                };

                _unitWork.InteractiveActivityrepo.add(activity);
            }

            await _unitWork.SaveAsync();

            return (true,
                "Recurring schedule updated. Existing booked sessions were preserved.");
        }


        // END AN ENTIRE RECURRING SCHEDULE
        public async Task<(bool Success, string Message)> EndScheduleAsync(
            int scheduleId,
            string instructorId)
        {
            var schedule = _unitWork.InteractiveScheduleRepo
                .GetById(scheduleId);

            if (schedule == null)
                return (false, "Recurring schedule not found.");

            if (schedule.InstructorId != instructorId)
                return (false, "You are not authorized to end this schedule.");

            if (!schedule.IsActive)
                return (false, "This schedule has already ended.");

            var now = DateTime.UtcNow;

            var activities = _unitWork.InteractiveActivityrepo
                .GetByCondition(a =>
                    a.InteractiveScheduleId == scheduleId);

            schedule.IsActive = false;

            foreach (var activity in activities)
            {
                var occurrenceStart = ConvertEgyptTimeToUtc(
    activity.Date,
    activity.StartTime);

                if (occurrenceStart <= now ||
                    activity.Status == SessionStatus.Cancelled ||
                    activity.Status == SessionStatus.Finished ||
                    activity.Status == SessionStatus.Completed)
                {
                    continue;
                }

                var bookings = _unitWork.InteractiveBookingrepo
                    .GetByCondition(b =>
                        b.InteractiveActivityId == activity.Id &&
                        b.Status != SessionStatus.Cancelled &&
                        b.ApprovalStatus != BookingApprovalStatus.Rejected);

                // Do not silently disrupt students who have bookings.
                // Preserve booked sessions as independent one-off sessions.
                if (bookings.Count > 0)
                {
                    activity.InteractiveScheduleId = null;
                    continue;
                }

                activity.Status = SessionStatus.Cancelled;
            }

            await _unitWork.SaveAsync();

            return (true,
                "Recurring schedule ended. Future unbooked sessions were cancelled.");
        }
        // ADMIN: APPROVE INDIVIDUAL ACTIVITY
        public async Task<(bool Success, string Message)>
            AdminApproveActivityAsync(int id)
        {
            var activity = _unitWork.InteractiveActivityrepo.GetById(id);

            if (activity == null)
                return (false, "Interactive activity not found.");

            if (activity.Status == SessionStatus.Cancelled)
                return (false, "Cancelled activities cannot be approved.");

            activity.ApprovalStatus = ActivityApprovalStatus.Approved;

            // Find students who have an approved, active package
            // that includes interactive activities.
            var eligibleEnrollments = _unitWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.TrackId == activity.TrackId &&
                    e.Status == SessionStatus.Active &&
                    e.ApprovalStatus == BookingApprovalStatus.Approved &&
                    (
                        e.BookingType == TrackBookingType.TrackWithInteractive ||
                        e.BookingType == TrackBookingType.TrackWithInteractiveAndWorkshop
                    ))
                .ToList();

            foreach (var enrollment in eligibleEnrollments)
            {
                // Check whether this student already has a booking.
                var existingBooking = _unitWork.InteractiveBookingrepo
                    .GetByCondition(b =>
                        b.UserId == enrollment.StudentId &&
                        b.InteractiveActivityId == activity.Id)
                    .FirstOrDefault();

                // Don't create duplicate bookings.
                if (existingBooking != null)
                    continue;

                var booking = new InteractiveBooking
                {
                    UserId = enrollment.StudentId,
                    InteractiveActivityId = activity.Id,
                    BookedAt = DateTime.UtcNow,
                    Status = SessionStatus.Scheduled,
                    Source = BookingSource.TrackBooking,
                    ApprovalStatus = BookingApprovalStatus.Approved
                };

                _unitWork.InteractiveBookingrepo.add(booking);
            }

            await _unitWork.SaveAsync();

            return (true, "Interactive activity approved and eligible package students were booked successfully.");
        }


        // ADMIN: REJECT INDIVIDUAL ACTIVITY
        public async Task<(bool Success, string Message)>
            AdminRejectActivityAsync(int id)
        {
            var activity = _unitWork.InteractiveActivityrepo.GetById(id);

            if (activity == null)
                return (false, "Interactive activity not found.");

            activity.ApprovalStatus = ActivityApprovalStatus.Rejected;

            await _unitWork.SaveAsync();

            return (true, "Interactive activity rejected successfully.");
        }


        // ADMIN: RESCHEDULE INDIVIDUAL ACTIVITY
        public async Task<(bool Success, string Message)>
            AdminRescheduleActivityAsync(
                int id,
                DateTime date,
                TimeSpan startTime,
                TimeSpan endTime)
        {
            var activity = _unitWork.InteractiveActivityrepo.GetById(id);

            if (activity == null)
                return (false, "Interactive activity not found.");

            if (activity.Status == SessionStatus.Cancelled)
                return (false, "Cancelled activities cannot be rescheduled.");

            if (date.Date < DateTime.UtcNow.Date)
                return (false, "Activity date cannot be in the past.");

            if (endTime <= startTime)
                return (false, "End time must be after start time.");

            activity.Date = date.Date;
            activity.StartTime = startTime;
            activity.EndTime = endTime;

            // Admin rescheduling does not reset approval status.
            // Existing bookings and schedule relationships are preserved.

            await _unitWork.SaveAsync();

            return (true, "Interactive activity rescheduled successfully.");
        }


        // ADMIN: CANCEL INDIVIDUAL ACTIVITY
        public async Task<(bool Success, string Message)>
            AdminCancelActivityAsync(int id)
        {
            var activity = _unitWork.InteractiveActivityrepo.GetById(id);

            if (activity == null)
                return (false, "Interactive activity not found.");

            if (activity.Status == SessionStatus.Cancelled)
                return (false, "Interactive activity is already cancelled.");

            activity.Status = SessionStatus.Cancelled;

            // ApprovalStatus remains independent of SessionStatus.
            // Existing bookings are preserved.

            await _unitWork.SaveAsync();

            return (true, "Interactive activity cancelled successfully.");
        }


        // ADMIN: APPROVE A RECURRING SCHEDULE
        public async Task<(bool Success, string Message)>
            AdminApproveScheduleAsync(int scheduleId)
        {
            var schedule = _unitWork.InteractiveScheduleRepo.GetById(scheduleId);

            if (schedule == null)
                return (false, "Recurring schedule not found.");

            var activities = _unitWork.InteractiveActivityrepo
                .GetByCondition(a => a.InteractiveScheduleId == scheduleId)
                .ToList();

            if (!activities.Any())
                return (false, "This schedule has no generated activities.");

            foreach (var activity in activities)
            {
                // Never approve cancelled or individually rejected occurrences.
                if (activity.Status == SessionStatus.Cancelled)
                    continue;

                if (activity.ApprovalStatus == ActivityApprovalStatus.Pending)
                    activity.ApprovalStatus = ActivityApprovalStatus.Approved;
            }

            await _unitWork.SaveAsync();

            return (true, "Pending activities in the recurring schedule were approved.");
        }


        // ADMIN: REJECT A RECURRING SCHEDULE
        public async Task<(bool Success, string Message)>
            AdminRejectScheduleAsync(int scheduleId)
        {
            var schedule = _unitWork.InteractiveScheduleRepo.GetById(scheduleId);

            if (schedule == null)
                return (false, "Recurring schedule not found.");

            var activities = _unitWork.InteractiveActivityrepo
                .GetByCondition(a => a.InteractiveScheduleId == scheduleId)
                .ToList();

            if (!activities.Any())
                return (false, "This schedule has no generated activities.");

            foreach (var activity in activities)
            {
                // Keep cancelled sessions cancelled.
                if (activity.Status == SessionStatus.Cancelled)
                    continue;

                activity.ApprovalStatus = ActivityApprovalStatus.Rejected;
            }

            await _unitWork.SaveAsync();

            return (true, "Recurring schedule activities were rejected.");
        }
        // GET ALL ACTIVITIES CREATED BY THE LOGGED-IN INSTRUCTOR
        public async Task<List<InteractiveActivityResponseDTO>>
            GetActivitiesByInstructorAsync(string instructorId)
        {
            var activities = _unitWork.InteractiveActivityrepo
                .GetByCondition(x => x.InstructorId == instructorId)
                .ToList();

            var result = _mapper.Map<List<InteractiveActivityResponseDTO>>(
                activities);

            foreach (var activity in result)
            {
                var currentBookings = _unitWork.InteractiveBookingrepo
                    .GetByCondition(b =>
                        b.InteractiveActivityId == activity.Id &&
                        b.Status != SessionStatus.Cancelled &&
                        b.ApprovalStatus != BookingApprovalStatus.Rejected)
                    .Count();

                activity.AvailableCapacity = Math.Max(
                    0,
                    activity.Capacity - currentBookings);
            }

            return result;
        }

    }
}