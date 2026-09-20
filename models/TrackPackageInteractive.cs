namespace Engineering_Hub.models
{
    public class TrackPackageInteractive
    {
        public int TrackPackageId { get; set; }

        public int InteractiveActivityId { get; set; }

        public TrackPackage TrackPackage { get; set; }

        public InteractiveActivity InteractiveActivity { get; set; }
    }
}