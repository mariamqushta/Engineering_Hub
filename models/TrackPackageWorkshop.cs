namespace Engineering_Hub.models
{
    public class TrackPackageWorkshop
    {
        public int TrackPackageId { get; set; }

        public int WorkshopId { get; set; }

        public TrackPackage TrackPackage { get; set; }

        public Workshop Workshop { get; set; }
    }
}