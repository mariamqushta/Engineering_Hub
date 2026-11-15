using System;

public class PrivateReplacement
{
    public int Id { get; set; }
    public int MissedOccurrenceId { get; set; }
    public int ReplacementOccurrenceId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string AssignedByAdminId { get; set; } = string.Empty;
    public DateTime AssignedAtUtc { get; set; }
    public PrivateOccurrence MissedOccurrence { get; set; } = null!;
    public PrivateOccurrence ReplacementOccurrence { get; set; } = null!;
}
