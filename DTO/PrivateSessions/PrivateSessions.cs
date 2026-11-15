
using Engineering_Hub.models;
using System;

namespace Engineering_Hub.DTOs.PrivateSessions
{
    // =========================
    // Availability
    // =========================

    public class CreatePrivateAvailabilityDto
    {
        public string InstructorId { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class PrivateAvailabilityDto
    {
        public int Id { get; set; }
        public string InstructorId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    // =========================
    // Arrangements
    // =========================

    public class CreatePrivateArrangementDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string InstructorId { get; set; } = string.Empty;
        public int TrackId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public string? MeetingLink { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PrivateArrangementDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string InstructorId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int TrackId { get; set; }
        public string? MeetingLink { get; set; }
        public string TrackTitle { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public BookingApprovalStatus ApprovalStatus { get; set; }
    }

    // =========================
    // Replacement
    // =========================

    public class AssignPrivateReplacementDto
    {
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string MeetingLink { get; set; } = string.Empty;
    }

    // =========================
    // Admin Occurrences
    // =========================

    public class AdminPrivateOccurrenceDto
    {
        public int Id { get; set; }
        public int? PrivateArrangementId { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;

        public string InstructorId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;

        public int TrackId { get; set; }
        public string TrackTitle { get; set; } = string.Empty;

        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public SessionStatus Status { get; set; }
        public bool IsReplacement { get; set; }
    }

    // =========================
    // Instructor Calendar
    // =========================

    public class InstructorPrivateOccurrenceDto
    {
        public int Id { get; set; }

        public string? StudentName { get; set; }

        public int TrackId { get; set; }
        public string TrackTitle { get; set; } = string.Empty;

        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public SessionStatus Status { get; set; }
        public bool IsReplacement { get; set; }
    }

    // =========================
    // Student Calendar
    // =========================

    public class StudentPrivateOccurrenceDto
    {
        public int Id { get; set; }

        public string InstructorName { get; set; } = string.Empty;

        public int TrackId { get; set; }
        public string TrackTitle { get; set; } = string.Empty;

        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public SessionStatus Status { get; set; }
        public bool IsReplacement { get; set; }
    }

    // =========================
    // Private Requests
    // =========================

    public class PrivateRequestDto
    {
        public int Id { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;

        public int OccurrenceId { get; set; }

        public string InstructorName { get; set; } = string.Empty;
        public string TrackTitle { get; set; } = string.Empty;

        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public RequestType RequestType { get; set; }
        public BookingApprovalStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class ReviewPrivateRequestDto
    {
        public BookingApprovalStatus Decision { get; set; }
    }

    // =========================
    // Occurrence Status
    // =========================

    public class UpdatePrivateOccurrenceStatusDto
    {
        public SessionStatus Status { get; set; }
    }
}
