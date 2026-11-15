using Engineering_Hub.models;
using System;
using System.Collections.Generic;

public class PrivateOccurrence
{
    public int Id { get; set; }
    public int? PrivateArrangementId { get; set; }   // null for replacement sessions
    public string StudentId { get; set; } = string.Empty;
    public string InstructorId { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public int TrackId { get; set; }

    public ApplicationUser Student { get; set; } = null!;
    public ApplicationUser Instructor { get; set; } = null!;
    public Track Track { get; set; } = null!;
    public string? MeetingLink { get; set; }
    public PrivateArrangement? PrivateArrangement { get; set; }

    public ICollection<PrivateRequest> Requests { get; set; }
        = new List<PrivateRequest>();

    public PrivateReplacement? Replacement { get; set; }
}
