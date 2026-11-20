using AutoMapper;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.Workshop;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace Engineering_Hub.Services
{
    public class WorkshopService : IWorkshopService
    {
        private readonly UnitWork _unitWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;
        public WorkshopService(
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
            int? excludedWorkshopId = null)
        {
            var newStartUtc = ConvertEgyptTimeToUtc(date, startTime);
            var newEndUtc = ConvertEgyptTimeToUtc(date, endTime);

            // 1. Check Interactive Activities.
            var activities = _unitWork.InteractiveActivityrepo
                .GetByCondition(x =>
                    x.InstructorId == instructorId &&
                    x.Status != SessionStatus.Cancelled &&
                    (x.ApprovalStatus == ActivityApprovalStatus.Pending ||
                     x.ApprovalStatus == ActivityApprovalStatus.Approved))
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

            // 2. Check other Workshops.
            var workshops = _unitWork.Workshoprepo
                .GetByCondition(x =>
                    x.InstructorId == instructorId &&
                    x.Status != SessionStatus.Cancelled &&
                    (x.ApprovalStatus == ActivityApprovalStatus.Pending ||
                     x.ApprovalStatus == ActivityApprovalStatus.Approved) &&
                    (!excludedWorkshopId.HasValue ||
                     x.Id != excludedWorkshopId.Value))
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
                    return "The instructor already has another workshop during this time.";
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
        public async Task<WorkshopResponseDTO?> CreateWorkshopAsync(
            WorkshopDTO dto,
            string instructorId)
        {
            // 1. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return null;

            // 2. Check instructor is assigned to Track
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
            // 7. Check instructor schedule conflicts
            var conflict = CheckInstructorScheduleConflict(
                instructorId,
                dto.Date,
                dto.StartTime,
                dto.EndTime);

            if (conflict != null)
                return null;
            // 7. Map DTO → Entity
            var workshop = _mapper.Map<Workshop>(dto);
            workshop.InstructorId = instructorId;
           
          

            // 8. Set backend-controlled values
            workshop.Status = SessionStatus.Scheduled;
            workshop.ApprovalStatus = ActivityApprovalStatus.Pending;
            // 9. Add
            _unitWork.Workshoprepo.add(workshop);

            // 10. Save
            await _unitWork.SaveAsync();

            // 11. Map Entity → Response DTO
            return _mapper.Map<WorkshopResponseDTO>(workshop);
        }


        // GET WORKSHOPS BY TRACK
        // GET WORKSHOPS BY TRACK
        public async Task<List<WorkshopResponseDTO>> GetWorkshopsByTrackAsync(
            int trackId)
        {
            var workshops = _unitWork.Workshoprepo
      .GetByCondition(w =>
          w.TrackId == trackId &&
          w.Status != SessionStatus.Cancelled &&
          w.ApprovalStatus == ActivityApprovalStatus.Approved)
      .ToList();

            var result = _mapper.Map<List<WorkshopResponseDTO>>(workshops);

            foreach (var workshopDto in result)
            {
                var activeBookings = _unitWork.WorkshopBookingrepo
                    .GetByCondition(b =>
                        b.WorkshopId == workshopDto.Id &&
                        b.Status != SessionStatus.Cancelled &&
                        b.ApprovalStatus != BookingApprovalStatus.Rejected)
                    .Count();

                workshopDto.AvailableCapacity = Math.Max(
                    0,
                    workshopDto.Capacity - activeBookings);
            }

            return result;
        }


        // UPDATE
        public async Task<(bool Success, string Message)> UpdateWorkshopAsync(
            int id,
            WorkshopDTO dto,
            string instructorId)
        {
            // 1. Find workshop
            var workshop = _unitWork.Workshoprepo
                .GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");

            // 2. Don't update cancelled workshop
            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "Cancelled workshop cannot be updated.");
            if (workshop.InstructorId != instructorId)
                return (false, "You are not authorized to edit this workshop.");

            if (dto.TrackId != workshop.TrackId)
                return (false, "A workshop's track cannot be changed.");
            // 3. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return (false, "Track not found or inactive.");

            // 4. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false, "You are not assigned to this Track.");

            // 5. Validate price
            if (dto.Price < 0)
                return (false, "Price cannot be negative.");

            // 6. Validate capacity
            if (dto.Capacity <= 0)
                return (false, "Capacity must be greater than zero.");

            // 7. Validate time
            if (dto.EndTime <= dto.StartTime)
                return (false, "End time must be after start time.");

            // 8. Validate date
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return (false, "Workshop date cannot be in the past.");
            // 9. Check instructor schedule conflicts
            var conflict = CheckInstructorScheduleConflict(
                instructorId,
                dto.Date,
                dto.StartTime,
                dto.EndTime,
                workshop.Id);

            if (conflict != null)
                return (false, conflict);
            // 9. Map DTO → existing entity
            _mapper.Map(dto, workshop);

            // Keep status controlled by backend
            // Don't allow update to change it.
            // Instructor edits require Admin approval again.
            workshop.ApprovalStatus = ActivityApprovalStatus.Pending;
            // 10. Save
            await _unitWork.SaveAsync();

            return (true, "Workshop updated successfully.");
        }


        // CANCEL
        public async Task<(bool Success, string Message)> CancelWorkshopAsync(
            int id,
            string instructorId)
        {
            // 1. Find workshop
            var workshop = _unitWork.Workshoprepo
                .GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");
            if (workshop.InstructorId != instructorId)
                return (false, "You are not authorized to cancel this workshop.");
            // 2. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == workshop.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false, "You are not assigned to this Track.");

            // 3. Check already cancelled
            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "Workshop is already cancelled.");

            // 4. Cancel
            workshop.Status = SessionStatus.Cancelled;

            // 5. Save
            await _unitWork.SaveAsync();

            return (true, "Workshop cancelled successfully.");
        }

        // ADMIN: APPROVE WORKSHOP
        public async Task<(bool Success, string Message)>
            AdminApproveWorkshopAsync(int id)
        {
            var workshop = _unitWork.Workshoprepo.GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");

            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "Cancelled workshops cannot be approved.");

            workshop.ApprovalStatus = ActivityApprovalStatus.Approved;

            await _unitWork.SaveAsync();

            return (true, "Workshop approved successfully.");
        }


        // ADMIN: REJECT WORKSHOP
        public async Task<(bool Success, string Message)>
            AdminRejectWorkshopAsync(int id)
        {
            var workshop = _unitWork.Workshoprepo.GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");

            workshop.ApprovalStatus = ActivityApprovalStatus.Rejected;

            await _unitWork.SaveAsync();

            return (true, "Workshop rejected successfully.");
        }


        // ADMIN: RESCHEDULE WORKSHOP
        public async Task<(bool Success, string Message)>
            AdminRescheduleWorkshopAsync(
                int id,
                DateTime date,
                TimeSpan startTime,
                TimeSpan endTime)
        {
            var workshop = _unitWork.Workshoprepo.GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");

            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "Cancelled workshops cannot be rescheduled.");

            if (date.Date < DateTime.UtcNow.Date)
                return (false, "Workshop date cannot be in the past.");

            if (endTime <= startTime)
                return (false, "End time must be after start time.");
            // Check instructor schedule conflicts before rescheduling
            if (!string.IsNullOrEmpty(workshop.InstructorId))
            {
                var conflict = CheckInstructorScheduleConflict(
                    workshop.InstructorId,
                    date,
                    startTime,
                    endTime,
                    workshop.Id);

                if (conflict != null)
                    return (false, conflict);
            }
            workshop.Date = date.Date;
            workshop.StartTime = startTime;
            workshop.EndTime = endTime;

            // Admin rescheduling does not reset approval status.
            // Existing bookings are preserved.

            await _unitWork.SaveAsync();

            return (true, "Workshop rescheduled successfully.");
        }


        // ADMIN: CANCEL WORKSHOP
        public async Task<(bool Success, string Message)>
            AdminCancelWorkshopAsync(int id)
        {
            var workshop = _unitWork.Workshoprepo.GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");

            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "Workshop is already cancelled.");

            workshop.Status = SessionStatus.Cancelled;

            // ApprovalStatus remains independent of SessionStatus.
            // Existing bookings are preserved.

            await _unitWork.SaveAsync();

            return (true, "Workshop cancelled successfully.");
        }
        // GET ALL WORKSHOPS CREATED BY THE LOGGED-IN INSTRUCTOR
        public async Task<List<WorkshopResponseDTO>>
            GetWorkshopsByInstructorAsync(string instructorId)
        {
            var workshops = _unitWork.Workshoprepo
                .GetByCondition(w => w.InstructorId == instructorId)
                .ToList();

            var result = _mapper.Map<List<WorkshopResponseDTO>>(workshops);

            foreach (var workshop in result)
            {
                var activeBookings = _unitWork.WorkshopBookingrepo
                    .GetByCondition(b =>
                        b.WorkshopId == workshop.Id &&
                        b.Status != SessionStatus.Cancelled &&
                        b.ApprovalStatus != BookingApprovalStatus.Rejected)
                    .Count();

                workshop.AvailableCapacity = Math.Max(
                    0,
                    workshop.Capacity - activeBookings);
            }

            return result;
        }
    }
}