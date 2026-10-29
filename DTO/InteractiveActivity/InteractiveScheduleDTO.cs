
using System;
using System.Collections.Generic;

namespace Engineering_Hub.DTO.InteractiveActivity
{
    public class InteractiveScheduleDTO
    {
        public int TrackId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string? MeetingUrl { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; } = new();

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
