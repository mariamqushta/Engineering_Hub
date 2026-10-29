namespace Engineering_Hub.models
{
    public enum BookingSource
    {
        Individual,
        TrackBooking
    }

    public enum BookingApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum SessionStatus
    {
        Scheduled,
        Ongoing,
        Finished,
        Cancelled,
        Active,
        Completed
    }
}
