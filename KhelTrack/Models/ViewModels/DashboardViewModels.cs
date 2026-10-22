namespace KhelTrack.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTournaments { get; set; }
        public int ActiveTournaments { get; set; }
        public int TotalTeams { get; set; }
        public int ApprovedTeams { get; set; }
        public int PendingTeams { get; set; }
        public int TotalMatches { get; set; }
        public int LiveMatchesCount { get; set; }
        public int CompletedMatchesCount { get; set; }
        public int TotalAthletes { get; set; }

        public List<MatchFixture> LiveMatches { get; set; } = new List<MatchFixture>();
        public List<MatchFixture> UpcomingFixtures { get; set; } = new List<MatchFixture>();
        public List<Tournament> FeaturedTournaments { get; set; } = new List<Tournament>();
        public List<AuditLog> RecentActivities { get; set; } = new List<AuditLog>();
    }

    public class TournamentLeaderboardItem
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int MatchesPlayed { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public int Drawn { get; set; }
        public int Points { get; set; }
        public string Form { get; set; } = string.Empty; // e.g. "W,W,L"
    }

    public class TournamentDetailsViewModel
    {
        public Tournament Tournament { get; set; } = null!;
        public List<Team> Teams { get; set; } = new List<Team>();
        public List<MatchFixture> Matches { get; set; } = new List<MatchFixture>();
        public List<TournamentLeaderboardItem> Leaderboard { get; set; } = new List<TournamentLeaderboardItem>();
        public bool IsUserCaptainRegistered { get; set; }
        public Team? UserTeam { get; set; }
    }

    public class AuditLogFilterViewModel
    {
        public string? ActionFilter { get; set; }
        public string? EntityTypeFilter { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<AuditLog> Logs { get; set; } = new List<AuditLog>();
        public List<string> AvailableActions { get; set; } = new List<string>();
        public List<string> AvailableEntityTypes { get; set; } = new List<string>();
    }
}
