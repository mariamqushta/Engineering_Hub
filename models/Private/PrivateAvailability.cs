using Engineering_Hub.models;
using System;

public class PrivateAvailability
{
    public int Id { get; set; }
    public string InstructorId { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }   // local admin time
    public TimeSpan EndTime { get; set; }     // local admin time
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public ApplicationUser Instructor { get; set; } = null!;
}
