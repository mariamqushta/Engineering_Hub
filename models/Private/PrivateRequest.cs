using Engineering_Hub.models;
using System;

public class PrivateRequest
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int OccurrenceId { get; set; }
    public RequestType RequestType { get; set; }
    public BookingApprovalStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public PrivateOccurrence Occurrence { get; set; } = null!;
}
