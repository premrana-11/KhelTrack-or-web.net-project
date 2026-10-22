namespace KhelTrack.Models
{
    public enum TournamentStatus
    {
        Upcoming,
        Active,
        Completed,
        Cancelled
    }

    public enum TeamStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum MatchStatus
    {
        Scheduled,
        Ongoing,
        Completed,
        Postponed,
        Cancelled
    }
}
