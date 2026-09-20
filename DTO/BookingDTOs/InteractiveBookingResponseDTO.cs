using Engineering_Hub.models;
using System;

namespace Engineering_Hub.DTO.BookingDTOs
{
    public class InteractiveBookingResponseDTO
    {
        public int BookingId { get; set; }
        public int InteractiveActivityId { get; set; }
        public DateTime BookedAt { get; set; }
        public SessionStatus BookingStatus { get; set; }

        public InteractiveActivityResponseDTO InteractiveActivity { get; set; }
    }
}