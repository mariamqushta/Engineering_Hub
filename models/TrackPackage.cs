using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class TrackPackage
    {
        public int Id { get; set; }

        public int TrackId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }


        // Relationships

        public Track Track { get; set; }

        public ICollection<TrackPackageBooking> Bookings { get; set; }

        public ICollection<TrackPackageWorkshop> TrackPackageWorkshops { get; set; }

        public ICollection<TrackPackageInteractive> TrackPackageInteractives { get; set; }
    }
}

