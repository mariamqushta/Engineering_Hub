using Engineering_Hub.models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IPrivateSessionService
{
    Task<PrivateArrangement> CreateArrangementAsync(
    string adminId,
    string studentId,
    string instructorId,
    int trackId,
    string meetingLink,
    DayOfWeek dayOfWeek,
    TimeSpan localStart,
    TimeSpan localEnd,
    DateTime startDate,
    DateTime? endDate);

    Task<int> GenerateOccurrencesAsync(int arrangementId, int weeksAhead);

    Task CancelOccurrenceAsync(int occurrenceId, string studentId);

    Task<PrivateReplacement> AssignReplacementAsync(string adminId,string meetingLink, int missedOccurrenceId,
        DateTime startUtc, DateTime endUtc);
    Task<List<PrivateOccurrence>> GetStudentOccurrencesAsync(string studentId, DateTime fromUtc, DateTime toUtc);
    Task<List<PrivateOccurrence>> GetInstructorOccurrencesAsync(string instructorId, DateTime fromUtc, DateTime toUtc);
    Task<PrivateAvailability> CreateAvailabilityAsync(string adminId, string instructorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, DateTime validFrom, DateTime? validTo);
    Task<List<PrivateAvailability>> GetInstructorAvailabilityAsync(string instructorId);
    Task DeactivateAvailabilityAsync(string adminId, int availabilityId);
    Task<List<PrivateArrangement>> GetAllArrangementsAsync();

    Task<List<PrivateOccurrence>> GetAdminOccurrencesAsync(
        DateTime fromUtc,
        DateTime toUtc,
        string? studentId = null,
        string? instructorId = null,
        SessionStatus? status = null);
    Task EndArrangementAsync(string adminId, int arrangementId);
    Task UpdateOccurrenceStatusAsync(
    string adminId,
    int occurrenceId,
    SessionStatus newStatus);
    Task<PrivateRequest> ReportStudentAbsenceAsync(
    string instructorId,
    int occurrenceId);

    Task<PrivateRequest> RequestInstructorCancellationAsync(
        string instructorId,
        int occurrenceId);

    Task<List<PrivateRequest>> GetAdminRequestsAsync(
        BookingApprovalStatus? status = null,
        RequestType? requestType = null);

    Task ReviewPrivateRequestAsync(
        string adminId,
        int requestId,
        BookingApprovalStatus decision);
}
