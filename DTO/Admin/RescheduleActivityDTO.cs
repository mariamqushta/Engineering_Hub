using System;

namespace Engineering_Hub.DTO.Admin
{
    public class RescheduleActivityDTO
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}