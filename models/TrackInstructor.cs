

namespace Engineering_Hub.models
{
    public class TrackInstructor
    {
        public string UserId { get; set; }

        public int TrackId { get; set; }


        // Navigation properties

        public ApplicationUser User { get; set; }

        public Track Track { get; set; }
    }
}
