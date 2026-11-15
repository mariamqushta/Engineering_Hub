using Engineering_Hub.models;
using System;
using System.Collections.Generic;

public class PrivateArrangement
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }   // local admin time
    public TimeSpan EndTime { get; set; }     // local admin time
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public BookingApprovalStatus ApprovalStatus { get; set; } = BookingApprovalStatus.Approved;
    public int TrackId { get; set; }
    public string? MeetingLink { get; set; }
    public ApplicationUser Student { get; set; } = null!;
    public ApplicationUser Instructor { get; set; } = null!;
    public Track Track { get; set; } = null!;

    public ICollection<PrivateOccurrence> Occurrences { get; set; }
        = new List<PrivateOccurrence>();
}
