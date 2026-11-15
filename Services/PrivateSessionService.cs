
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class PrivateSessionService : IPrivateSessionService
    {
        private static readonly TimeSpan MinDuration = TimeSpan.FromHours(1);
        private static readonly TimeSpan MaxDuration = TimeSpan.FromHours(2);
        private static readonly TimeSpan CancellationCutoff = TimeSpan.FromHours(3);

        private readonly UnitWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private TimeZoneInfo? _adminZone;

        public PrivateSessionService(
            UnitWork uow,
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _uow = uow;
            _userManager = userManager;
            _config = config;
        }

        private TimeZoneInfo AdminZone =>
            _adminZone ??= TimeZoneInfo.FindSystemTimeZoneById(
                _config["PrivateSessions:AdminTimeZoneId"]
                ?? throw new InvalidOperationException(
                    "PrivateSessions:AdminTimeZoneId is not configured."));

        // =========================================
        // ARRANGEMENTS
        // =========================================

        public async Task<PrivateArrangement> CreateArrangementAsync(
              string adminId,
                string studentId,
                string instructorId,
                int trackId,
                string meetingLink,
                DayOfWeek dayOfWeek,
                TimeSpan localStart,
                TimeSpan localEnd,
                DateTime startDate,
                DateTime? endDate)
        {
            await EnsureRoleAsync(adminId, "Admin");

            ValidateDuration(localStart, localEnd);
            ValidateDayOfWeek(dayOfWeek);

            if (endDate.HasValue &&
                endDate.Value.Date < startDate.Date)
            {
                throw new ArgumentException(
                    "End date cannot be before start date.");
            }

            var student = await _userManager.FindByIdAsync(studentId)
                ?? throw new KeyNotFoundException("Student not found.");

            if (!student.IsActive)
                throw new InvalidOperationException("Student account is inactive.");

            if (student.AccountType != AccountType.PrivateStudent)
            {
                throw new InvalidOperationException(
                    "Arrangements can only be created for private students.");
            }

            var instructor = await GetActiveInstructorAsync(instructorId);

            var trackExists = await _uow.Trackrepo.Query()
                .AnyAsync(t => t.Id == trackId && t.IsActive);

            if (!trackExists)
                throw new KeyNotFoundException("Track not found or inactive.");

            await EnsureInstructorAssignedToTrackAsync(instructorId, trackId);

            var overlappingArrangement =
                await _uow.PrivateArrangementrepo.Query()
                    .AnyAsync(a =>
                        a.IsActive &&
                        (a.StudentId == studentId ||
                         a.InstructorId == instructorId) &&
                        a.DayOfWeek == dayOfWeek &&
                        a.StartTime < localEnd &&
                        a.EndTime > localStart &&
                        a.StartDate <= (endDate ?? DateTime.MaxValue) &&
                        (a.EndDate == null ||
                         a.EndDate >= startDate.Date));

            if (overlappingArrangement)
            {
                throw new InvalidOperationException(
                    "This slot overlaps an existing arrangement for the student or instructor.");
            }

            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var arrangement = new PrivateArrangement
                {
                    StudentId = studentId,
                    InstructorId = instructorId,
                    TrackId = trackId,
                    DayOfWeek = dayOfWeek,
                    StartTime = localStart,
                    EndTime = localEnd,
                    StartDate = startDate.Date,
                    EndDate = endDate?.Date,
                    MeetingLink = meetingLink,
                    IsActive = true,
                    ApprovalStatus = BookingApprovalStatus.Approved
                };

                _uow.PrivateArrangementrepo.add(arrangement);
                await _uow.SaveAsync();

                // Generate the initial 12-week schedule.
                await GenerateOccurrencesCoreAsync(arrangement, 12);

                await transaction.CommitAsync();

                return arrangement;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> GenerateOccurrencesAsync(
            int arrangementId,
            int weeksAhead)
        {
            if (weeksAhead < 1 || weeksAhead > 52)
            {
                throw new ArgumentException(
                    "Weeks ahead must be between 1 and 52.");
            }

            var arrangement = await _uow.PrivateArrangementrepo.Query()
                .FirstOrDefaultAsync(a => a.Id == arrangementId)
                ?? throw new KeyNotFoundException("Arrangement not found.");

            if (!arrangement.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot generate sessions for an inactive arrangement.");
            }

            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var created = await GenerateOccurrencesCoreAsync(
                    arrangement, weeksAhead);

                await transaction.CommitAsync();

                return created;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<int> GenerateOccurrencesCoreAsync(
            PrivateArrangement arrangement,
            int weeksAhead)
        {
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow, AdminZone).Date;

            var from = arrangement.StartDate.Date > todayLocal
                ? arrangement.StartDate.Date
                : todayLocal;

            // Use a rolling window from today (or the arrangement's
            // future start date). Existing occurrences are skipped,
            // so missing dates inside the window can be filled.
            var to = from.AddDays(7 * weeksAhead);

            if (arrangement.EndDate.HasValue)
            {
                var exclusiveEnd = arrangement.EndDate.Value.Date.AddDays(1);

                if (exclusiveEnd < to)
                    to = exclusiveEnd;
            }

            var created = 0;

            for (var day = from; day < to; day = day.AddDays(1))
            {
                if (day.DayOfWeek != arrangement.DayOfWeek)
                    continue;

                var startUtc = LocalToUtc(day, arrangement.StartTime);
                var endUtc = LocalToUtc(day, arrangement.EndTime);

                var exists = await _uow.PrivateOccurrencerepo.Query()
                    .AnyAsync(o =>
                        o.PrivateArrangementId == arrangement.Id &&
                        o.StartUtc == startUtc);

                if (exists)
                    continue;

                if (!await InstructorIsAvailableAtUtcAsync(
                    arrangement.InstructorId, startUtc, endUtc))
                {
                    throw new InvalidOperationException(
                        $"Instructor availability does not cover {day:yyyy-MM-dd}.");
                }

                if (await HasConflictAsync(
                    arrangement.StudentId,
                    arrangement.InstructorId,
                    startUtc,
                    endUtc))
                {
                    throw new InvalidOperationException(
                        $"A student or instructor session conflicts with {day:yyyy-MM-dd}.");
                }

                _uow.PrivateOccurrencerepo.add(new PrivateOccurrence
                {
                    PrivateArrangementId = arrangement.Id,
                    StudentId = arrangement.StudentId,
                    InstructorId = arrangement.InstructorId,
                    TrackId = arrangement.TrackId,
                    StartUtc = startUtc,
                    EndUtc = endUtc,
                    Status = SessionStatus.Scheduled,
                    MeetingLink = arrangement.MeetingLink,
                });

                created++;
            }

            await _uow.SaveAsync();

            return created;
        }

        // =========================================
        // STUDENT CANCELLATION
        // =========================================

        public async Task CancelOccurrenceAsync(
            int occurrenceId,
            string studentId)
        {
            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var occurrence = await _uow.PrivateOccurrencerepo.Query()
                    .FirstOrDefaultAsync(o => o.Id == occurrenceId)
                    ?? throw new KeyNotFoundException("Session not found.");

                if (occurrence.StudentId != studentId)
                {
                    throw new UnauthorizedAccessException(
                        "You can only cancel your own sessions.");
                }

                if (occurrence.Status != SessionStatus.Scheduled)
                {
                    throw new InvalidOperationException(
                        "Only scheduled sessions can be cancelled.");
                }

                if (occurrence.StartUtc - DateTime.UtcNow <
                    CancellationCutoff)
                {
                    throw new InvalidOperationException(
                        "Sessions must be cancelled at least 3 hours before the start time.");
                }

                occurrence.Status = SessionStatus.Cancelled;

                _uow.PrivateRequestrepo.add(new PrivateRequest
                {
                    StudentId = studentId,
                    OccurrenceId = occurrence.Id,
                    RequestType = RequestType.Cancellation,
                    Status = BookingApprovalStatus.Approved,
                    CreatedAtUtc = DateTime.UtcNow
                });

                await _uow.SaveAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================
        // REPLACEMENT ASSIGNMENT
        // =========================================

        public async Task<PrivateReplacement> AssignReplacementAsync(
            string adminId,
            string meetingLink,
            int missedOccurrenceId,
            DateTime startUtc,
            DateTime endUtc)
        {
            await EnsureRoleAsync(adminId, "Admin");

            EnsureUtc(startUtc, nameof(startUtc));
            EnsureUtc(endUtc, nameof(endUtc));
            ValidateDuration(startUtc, endUtc);

            if (startUtc <= DateTime.UtcNow)
            {
                throw new ArgumentException(
                    "Replacement session must be scheduled in the future.");
            }

            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var missed = await _uow.PrivateOccurrencerepo.Query()
                    .FirstOrDefaultAsync(o => o.Id == missedOccurrenceId)
                    ?? throw new KeyNotFoundException("Session not found.");

                if (missed.Status != SessionStatus.Cancelled &&
                    missed.Status != SessionStatus.Missed)
                {
                    throw new InvalidOperationException(
                        "Only missed or cancelled sessions are eligible for replacement.");
                }

                var student = await _userManager.FindByIdAsync(missed.StudentId)
                    ?? throw new KeyNotFoundException("Student not found.");

                if (!student.IsActive ||
                    student.AccountType != AccountType.PrivateStudent)
                {
                    throw new InvalidOperationException(
                        "Replacement sessions can only be assigned to active private students.");
                }

                await GetActiveInstructorAsync(missed.InstructorId);
                await EnsureInstructorAssignedToTrackAsync(
                    missed.InstructorId, missed.TrackId);

                var alreadyReplaced =
                    await _uow.PrivateReplacementrepo.Query()
                        .AnyAsync(r => r.MissedOccurrenceId == missed.Id);

                if (alreadyReplaced)
                {
                    throw new InvalidOperationException(
                        "This session already has a replacement.");
                }

                if (!await InstructorIsAvailableAtUtcAsync(
                    missed.InstructorId, startUtc, endUtc))
                {
                    throw new InvalidOperationException(
                        "Instructor is not available for the replacement slot.");
                }

                if (await HasConflictAsync(
                    missed.StudentId,
                    missed.InstructorId,
                    startUtc,
                    endUtc))
                {
                    throw new InvalidOperationException(
                        "Replacement overlaps another session for the student or instructor.");
                }

                var replacementOccurrence = new PrivateOccurrence
                {
                    PrivateArrangementId = null,
                    StudentId = missed.StudentId,
                    InstructorId = missed.InstructorId,
                    StartUtc = startUtc,
                    EndUtc = endUtc,
                    TrackId = missed.TrackId,
                    Status = SessionStatus.Scheduled,
                    MeetingLink = meetingLink,
                };

                _uow.PrivateOccurrencerepo.add(replacementOccurrence);
                await _uow.SaveAsync();

                var replacement = new PrivateReplacement
                {
                    MissedOccurrenceId = missed.Id,
                    ReplacementOccurrenceId = replacementOccurrence.Id,
                    StudentId = missed.StudentId,
                    AssignedByAdminId = adminId,
                    AssignedAtUtc = DateTime.UtcNow
                };

                _uow.PrivateReplacementrepo.add(replacement);
                await _uow.SaveAsync();

                await transaction.CommitAsync();

                return replacement;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================
        // AVAILABILITY
        // =========================================

        public async Task<PrivateAvailability> CreateAvailabilityAsync(
            string adminId,
            string instructorId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            DateTime validFrom,
            DateTime? validTo)
        {
            await EnsureRoleAsync(adminId, "Admin");
            ValidateDayOfWeek(dayOfWeek);
            ValidateAvailabilityTime(startTime, endTime);

            if (validTo.HasValue &&
                validTo.Value.Date < validFrom.Date)
            {
                throw new ArgumentException(
                    "Valid-to date cannot be before valid-from date.");
            }

            await GetActiveInstructorAsync(instructorId);

            var overlaps = await _uow.PrivateAvailabilityrepo.Query()
                .AnyAsync(av =>
                    av.InstructorId == instructorId &&
                    av.IsActive &&
                    av.DayOfWeek == dayOfWeek &&
                    av.StartTime < endTime &&
                    av.EndTime > startTime &&
                    (validTo == null ||
                     av.ValidFrom <= validTo.Value.Date) &&
                    (av.ValidTo == null ||
                     av.ValidTo >= validFrom.Date));

            if (overlaps)
            {
                throw new InvalidOperationException(
                    "This availability overlaps an existing active availability slot.");
            }

            var availability = new PrivateAvailability
            {
                InstructorId = instructorId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                ValidFrom = validFrom.Date,
                ValidTo = validTo?.Date,
                IsActive = true
            };

            _uow.PrivateAvailabilityrepo.add(availability);
            await _uow.SaveAsync();

            return availability;
        }

        public async Task<List<PrivateAvailability>>
            GetInstructorAvailabilityAsync(string instructorId)
        {
            return await _uow.PrivateAvailabilityrepo.Query()
                .Include(av => av.Instructor)
                .Where(av => av.InstructorId == instructorId)
                .OrderBy(av => av.DayOfWeek)
                .ThenBy(av => av.StartTime)
                .ToListAsync();
        }

        public async Task DeactivateAvailabilityAsync(
            string adminId,
            int availabilityId)
        {
            await EnsureRoleAsync(adminId, "Admin");

            var slot = await _uow.PrivateAvailabilityrepo.Query()
                .FirstOrDefaultAsync(av => av.Id == availabilityId)
                ?? throw new KeyNotFoundException(
                    "Availability slot not found.");

            if (!slot.IsActive)
                throw new InvalidOperationException(
                    "This availability slot is already inactive.");

            slot.IsActive = false;
            await _uow.SaveAsync();
        }

        // =========================================
        // CALENDAR QUERIES
        // =========================================

        public async Task<List<PrivateOccurrence>>
            GetStudentOccurrencesAsync(
                string studentId,
                DateTime fromUtc,
                DateTime toUtc)
        {
            ValidateUtcRange(fromUtc, toUtc);

            return await _uow.PrivateOccurrencerepo.Query()
                .Include(o => o.Instructor)
                .Include(o => o.Track)
                .Include(o => o.Replacement)
                .Where(o =>
                    o.StudentId == studentId &&
                    o.StartUtc < toUtc &&
                    o.EndUtc > fromUtc)
                .OrderBy(o => o.StartUtc)
                .ToListAsync();
        }

        public async Task<List<PrivateOccurrence>>
            GetInstructorOccurrencesAsync(
                string instructorId,
                DateTime fromUtc,
                DateTime toUtc)
        {
            ValidateUtcRange(fromUtc, toUtc);

            return await _uow.PrivateOccurrencerepo.Query()
                .Include(o => o.Student)
                .Include(o => o.Replacement)
                .Include(o => o.Track)
                .Where(o =>
                    o.InstructorId == instructorId &&
                    o.StartUtc < toUtc &&
                    o.EndUtc > fromUtc)
                .OrderBy(o => o.StartUtc)
                .ToListAsync();
        }

        public async Task<List<PrivateArrangement>>
            GetAllArrangementsAsync()
        {
            return await _uow.PrivateArrangementrepo.Query()
                .Include(a => a.Student)
                .Include(a => a.Instructor)
                .Include(a => a.Track)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();
        }

        public async Task<List<PrivateOccurrence>>
            GetAdminOccurrencesAsync(
                DateTime fromUtc,
                DateTime toUtc,
                string? studentId = null,
                string? instructorId = null,
                SessionStatus? status = null)
        {
            ValidateUtcRange(fromUtc, toUtc);

            var query = _uow.PrivateOccurrencerepo.Query()
                .Include(o => o.Student)
                .Include(o => o.Instructor)
                .Include(o => o.Replacement)
                .Include(o => o.Track)
                .Where(o =>
                    o.StartUtc < toUtc &&
                    o.EndUtc > fromUtc);

            if (!string.IsNullOrWhiteSpace(studentId))
                query = query.Where(o => o.StudentId == studentId);

            if (!string.IsNullOrWhiteSpace(instructorId))
                query = query.Where(o => o.InstructorId == instructorId);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            return await query
                .OrderBy(o => o.StartUtc)
                .ToListAsync();
        }

        // =========================================
        // END ARRANGEMENT
        // =========================================

        public async Task EndArrangementAsync(
            string adminId,
            int arrangementId)
        {
            await EnsureRoleAsync(adminId, "Admin");

            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var arrangement = await _uow.PrivateArrangementrepo.Query()
                    .FirstOrDefaultAsync(a => a.Id == arrangementId)
                    ?? throw new KeyNotFoundException(
                        "Arrangement not found.");

                if (!arrangement.IsActive)
                {
                    throw new InvalidOperationException(
                        "This arrangement is already inactive.");
                }

                arrangement.IsActive = false;

                var now = DateTime.UtcNow;

                var futureOccurrences =
                    await _uow.PrivateOccurrencerepo.Query()
                        .Where(o =>
                            o.PrivateArrangementId == arrangementId &&
                            o.StartUtc > now &&
                            o.Status == SessionStatus.Scheduled)
                        .ToListAsync();

                foreach (var occurrence in futureOccurrences)
                    occurrence.Status = SessionStatus.Cancelled;

                await _uow.SaveAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================
        // ADMIN OCCURRENCE STATUS
        // =========================================


        public async Task UpdateOccurrenceStatusAsync(
            string adminId,
            int occurrenceId,
            SessionStatus newStatus)
        {
            await EnsureRoleAsync(adminId, "Admin");

            if (newStatus != SessionStatus.Completed &&
                newStatus != SessionStatus.Missed &&
                newStatus != SessionStatus.Cancelled)
            {
                throw new ArgumentException(
                    "Admin can only mark sessions as Completed, Missed, or Cancelled.");
            }

            var occurrence = await _uow.PrivateOccurrencerepo.Query()
                .FirstOrDefaultAsync(o => o.Id == occurrenceId)
                ?? throw new KeyNotFoundException("Session not found.");

            if (!IsScheduledOrOngoing(occurrence.Status))
            {
                throw new InvalidOperationException(
                    "Only scheduled or ongoing sessions can be updated.");
            }

            var now = DateTime.UtcNow;

            if (newStatus == SessionStatus.Completed &&
                occurrence.EndUtc > now)
            {
                throw new InvalidOperationException(
                    "A session cannot be marked completed before its scheduled end time.");
            }

            if (newStatus == SessionStatus.Missed &&
                occurrence.StartUtc > now)
            {
                throw new InvalidOperationException(
                    "A session cannot be marked missed before its scheduled start time.");
            }

            occurrence.Status = newStatus;

            await _uow.SaveAsync();
        }


        // =========================================
        // INSTRUCTOR REQUESTS
        // =========================================

        public async Task<PrivateRequest> ReportStudentAbsenceAsync(
            string instructorId,
            int occurrenceId)
        {
            await GetActiveInstructorAsync(instructorId);

            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var occurrence = await _uow.PrivateOccurrencerepo.Query()
                    .FirstOrDefaultAsync(o => o.Id == occurrenceId)
                    ?? throw new KeyNotFoundException("Session not found.");

                if (occurrence.InstructorId != instructorId)
                {
                    throw new UnauthorizedAccessException(
                        "You can only report absence for your own sessions.");
                }

                if (!IsScheduledOrOngoing(occurrence.Status))
                {
                    throw new InvalidOperationException(
                        "Absence can only be reported for a scheduled or ongoing session.");
                }

                if (occurrence.StartUtc > DateTime.UtcNow)
                {
                    throw new InvalidOperationException(
                        "Student absence can only be reported after the session has started.");
                }

                var pendingExists =
                    await _uow.PrivateRequestrepo.Query()
                        .AnyAsync(r =>
                            r.OccurrenceId == occurrenceId &&
                            r.RequestType == RequestType.StudentAbsence &&
                            r.Status == BookingApprovalStatus.Pending);

                if (pendingExists)
                {
                    throw new InvalidOperationException(
                        "An absence report is already pending for this session.");
                }

                var request = new PrivateRequest
                {
                    StudentId = occurrence.StudentId,
                    OccurrenceId = occurrence.Id,
                    RequestType = RequestType.StudentAbsence,
                    Status = BookingApprovalStatus.Pending,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _uow.PrivateRequestrepo.add(request);
                await _uow.SaveAsync();
                await transaction.CommitAsync();

                return request;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PrivateRequest>
            RequestInstructorCancellationAsync(
                string instructorId,
                int occurrenceId)
        {
            await GetActiveInstructorAsync(instructorId);

            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var occurrence = await _uow.PrivateOccurrencerepo.Query()
                    .FirstOrDefaultAsync(o => o.Id == occurrenceId)
                    ?? throw new KeyNotFoundException("Session not found.");

                if (occurrence.InstructorId != instructorId)
                {
                    throw new UnauthorizedAccessException(
                        "You can only request cancellation for your own sessions.");
                }

                if (occurrence.Status != SessionStatus.Scheduled)
                {
                    throw new InvalidOperationException(
                        "Only scheduled sessions can be cancelled by instructor request.");
                }

                if (occurrence.StartUtc <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException(
                        "You cannot request cancellation after the session has started.");
                }

                var pendingExists =
                    await _uow.PrivateRequestrepo.Query()
                        .AnyAsync(r =>
                            r.OccurrenceId == occurrenceId &&
                            r.RequestType == RequestType.InstructorCancellation &&
                            r.Status == BookingApprovalStatus.Pending);

                if (pendingExists)
                {
                    throw new InvalidOperationException(
                        "A cancellation request is already pending for this session.");
                }

                var request = new PrivateRequest
                {
                    StudentId = occurrence.StudentId,
                    OccurrenceId = occurrence.Id,
                    RequestType = RequestType.InstructorCancellation,
                    Status = BookingApprovalStatus.Pending,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _uow.PrivateRequestrepo.add(request);
                await _uow.SaveAsync();
                await transaction.CommitAsync();

                return request;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================
        // ADMIN REQUESTS
        // =========================================

        public async Task<List<PrivateRequest>> GetAdminRequestsAsync(
            BookingApprovalStatus? status = null,
            RequestType? requestType = null)
        {
            IQueryable<PrivateRequest> query =
                _uow.PrivateRequestrepo.Query()
                     .Include(r => r.Occurrence)
                    .ThenInclude(r => r.Student)
                    .Include(r => r.Occurrence)
                        .ThenInclude(o => o.Student)
                    .Include(r => r.Occurrence)
                        .ThenInclude(o => o.Instructor)
                    .Include(r => r.Occurrence)
                        .ThenInclude(o => o.Track);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (requestType.HasValue)
                query = query.Where(r => r.RequestType == requestType.Value);

            return await query
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task ReviewPrivateRequestAsync(
            string adminId,
            int requestId,
            BookingApprovalStatus decision)
        {
            await EnsureRoleAsync(adminId, "Admin");

            if (decision != BookingApprovalStatus.Approved &&
                decision != BookingApprovalStatus.Rejected)
            {
                throw new ArgumentException(
                    "Admin can only approve or reject a request.");
            }

            await using var transaction =
                await _uow.BeginTransactionAsync();

            try
            {
                var request = await _uow.PrivateRequestrepo.Query()
                    .Include(r => r.Occurrence)
                    .FirstOrDefaultAsync(r => r.Id == requestId)
                    ?? throw new KeyNotFoundException("Request not found.");

                if (request.Status != BookingApprovalStatus.Pending)
                {
                    throw new InvalidOperationException(
                        "This request has already been reviewed.");
                }

                if (request.RequestType == RequestType.Cancellation)
                {
                    throw new InvalidOperationException(
                        "Student cancellations are automatic and cannot be reviewed.");
                }

                var occurrence = request.Occurrence;
                var now = DateTime.UtcNow;

                if (decision == BookingApprovalStatus.Approved)
                {
                    if (request.RequestType == RequestType.StudentAbsence)
                    {
                        if (!IsScheduledOrOngoing(occurrence.Status))
                        {
                            throw new InvalidOperationException(
                                "This session can no longer be marked as missed.");
                        }

                        if (occurrence.StartUtc > now)
                        {
                            throw new InvalidOperationException(
                                "A future session cannot be marked as missed.");
                        }

                        occurrence.Status = SessionStatus.Missed;
                    }
                    else if (request.RequestType ==
                             RequestType.InstructorCancellation)
                    {
                        if (occurrence.Status != SessionStatus.Scheduled)
                        {
                            throw new InvalidOperationException(
                                "Only scheduled sessions can be cancelled.");
                        }

                        if (occurrence.StartUtc <= now)
                        {
                            throw new InvalidOperationException(
                                "This session has already started.");
                        }

                        occurrence.Status = SessionStatus.Cancelled;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Unknown private request type.");
                    }
                }

                // Rejection changes only the request status.
                request.Status = decision;

                await _uow.SaveAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================
        // VALIDATION HELPERS
        // =========================================

        private static bool IsScheduledOrOngoing(SessionStatus status)
        {
            return status == SessionStatus.Scheduled ||
                   status == SessionStatus.Active ||
                   status == SessionStatus.Ongoing;
        }

        private static void ValidateDayOfWeek(DayOfWeek dayOfWeek)
        {
            if (!Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
                throw new ArgumentException("Invalid day of week.");
        }

        private static void ValidateDuration(
            TimeSpan start,
            TimeSpan end)
        {
            if (start < TimeSpan.Zero ||
                start >= TimeSpan.FromDays(1) ||
                end <= TimeSpan.Zero ||
                end > TimeSpan.FromDays(1) ||
                end <= start)
            {
                throw new ArgumentException(
                    "Invalid session start or end time.");
            }

            var duration = end - start;

            if (duration < MinDuration || duration > MaxDuration)
            {
                throw new ArgumentException(
                    "Sessions must last between 1 and 2 hours.");
            }
        }

        private static void ValidateAvailabilityTime(
            TimeSpan start,
            TimeSpan end)
        {
            if (start < TimeSpan.Zero ||
                start >= TimeSpan.FromDays(1) ||
                end <= TimeSpan.Zero ||
                end > TimeSpan.FromDays(1) ||
                end <= start)
            {
                throw new ArgumentException(
                    "Availability end time must be after its start time.");
            }
        }

        private static void ValidateDuration(
            DateTime startUtc,
            DateTime endUtc)
        {
            if (endUtc <= startUtc)
                throw new ArgumentException("Invalid session time range.");

            var duration = endUtc - startUtc;

            if (duration < MinDuration || duration > MaxDuration)
            {
                throw new ArgumentException(
                    "Sessions must last between 1 and 2 hours.");
            }
        }

        private static void EnsureUtc(DateTime value, string parameterName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    $"{parameterName} must be provided in UTC.");
            }
        }

        private static void ValidateUtcRange(
            DateTime fromUtc,
            DateTime toUtc)
        {
            EnsureUtc(fromUtc, nameof(fromUtc));
            EnsureUtc(toUtc, nameof(toUtc));

            if (fromUtc >= toUtc)
            {
                throw new ArgumentException(
                    "The end date must be after the start date.");
            }
        }

        private DateTime LocalToUtc(DateTime date, TimeSpan time)
        {
            var local = DateTime.SpecifyKind(
                date.Date + time,
                DateTimeKind.Unspecified);

            if (AdminZone.IsInvalidTime(local))
            {
                throw new InvalidOperationException(
                    $"The time {time:hh\\:mm} on {date:yyyy-MM-dd} does not exist in the admin time zone because of a clock change.");
            }

            if (AdminZone.IsAmbiguousTime(local))
            {
                throw new InvalidOperationException(
                    $"The time {time:hh\\:mm} on {date:yyyy-MM-dd} is ambiguous because of a clock change. Please choose another time.");
            }

            return TimeZoneInfo.ConvertTimeToUtc(local, AdminZone);
        }

        // =========================================
        // DATABASE VALIDATION HELPERS
        // =========================================

        private async Task<bool> HasConflictAsync(
            string studentId,
            string instructorId,
            DateTime startUtc,
            DateTime endUtc)
        {
            return await _uow.PrivateOccurrencerepo.Query()
                .AnyAsync(o =>
                    o.Status != SessionStatus.Cancelled &&
                    (o.StudentId == studentId ||
                     o.InstructorId == instructorId) &&
                    o.StartUtc < endUtc &&
                    o.EndUtc > startUtc);
        }

        private async Task<bool> InstructorIsAvailableAsync(
            string instructorId,
            DayOfWeek day,
            TimeSpan start,
            TimeSpan end,
            DateTime date)
        {
            return await _uow.PrivateAvailabilityrepo.Query()
                .AnyAsync(av =>
                    av.InstructorId == instructorId &&
                    av.IsActive &&
                    av.DayOfWeek == day &&
                    av.StartTime <= start &&
                    av.EndTime >= end &&
                    av.ValidFrom <= date.Date &&
                    (av.ValidTo == null ||
                     av.ValidTo >= date.Date));
        }

        private async Task<bool> InstructorIsAvailableAtUtcAsync(
            string instructorId,
            DateTime startUtc,
            DateTime endUtc)
        {
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(
                startUtc, AdminZone);

            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(
                endUtc, AdminZone);

            // Private sessions must fit inside one local calendar day.
            if (localStart.Date != localEnd.Date)
                return false;

            return await InstructorIsAvailableAsync(
                instructorId,
                localStart.DayOfWeek,
                localStart.TimeOfDay,
                localEnd.TimeOfDay,
                localStart.Date);
        }

        private async Task<ApplicationUser> GetActiveInstructorAsync(
            string instructorId)
        {
            var instructor = await _userManager.FindByIdAsync(instructorId)
                ?? throw new KeyNotFoundException("Instructor not found.");

            if (!instructor.IsActive)
            {
                throw new InvalidOperationException(
                    "Instructor account is inactive.");
            }

            if (!await _userManager.IsInRoleAsync(instructor, "Instructor"))
            {
                throw new InvalidOperationException(
                    "Selected user is not an instructor.");
            }

            return instructor;
        }

        private async Task EnsureInstructorAssignedToTrackAsync(
            string instructorId,
            int trackId)
        {
            var assigned = await _uow.TrackInstructorrepo.Query()
                .AnyAsync(t =>
                    t.UserId == instructorId &&
                    t.TrackId == trackId);

            if (!assigned)
            {
                throw new InvalidOperationException(
                    "Instructor is not assigned to this Track.");
            }
        }

        private async Task EnsureRoleAsync(
            string userId,
            string role)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                throw new UnauthorizedAccessException(
                    $"{role} role required.");
            }
        }
    }
}
