using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhelTrack.Models
{
    public class MatchFixture
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tournament")]
        public int TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public virtual Tournament? Tournament { get; set; }

        [StringLength(50)]
        [Display(Name = "Round / Stage")]
        public string RoundName { get; set; } = "Group Stage"; // Group Stage, Quarter-Final, Semi-Final, Final

        [Required]
        [Display(Name = "Team A")]
        public int TeamAId { get; set; }

        [ForeignKey("TeamAId")]
        public virtual Team? TeamA { get; set; }

        [Required]
        [Display(Name = "Team B")]
        public int TeamBId { get; set; }

        [ForeignKey("TeamBId")]
        public virtual Team? TeamB { get; set; }

        [Required(ErrorMessage = "Match date & time is required")]
        [Display(Name = "Match Date & Time")]
        public DateTime MatchDate { get; set; }

        [Required(ErrorMessage = "Ground/Venue is required")]
        [StringLength(100)]
        [Display(Name = "Ground / Court / Field")]
        public string Ground { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Team A Score")]
        public string? TeamAScore { get; set; }

        [StringLength(50)]
        [Display(Name = "Team B Score")]
        public string? TeamBScore { get; set; }

        [Display(Name = "Winner Team")]
        public int? WinnerTeamId { get; set; }

        [ForeignKey("WinnerTeamId")]
        public virtual Team? WinnerTeam { get; set; }

        [Display(Name = "Match Status")]
        public MatchStatus MatchStatus { get; set; } = MatchStatus.Scheduled;

        [StringLength(500)]
        [Display(Name = "Highlights / Result Summary")]
        public string? HighlightsSummary { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
