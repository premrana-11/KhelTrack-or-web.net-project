using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KhelTrack.Models.ViewModels
{
    public class CreateMatchFixtureViewModel
    {
        [Required(ErrorMessage = "Please select a tournament")]
        [Display(Name = "Tournament")]
        public int TournamentId { get; set; }

        [Required(ErrorMessage = "Round name is required")]
        [StringLength(50)]
        [Display(Name = "Tournament Round / Stage")]
        public string RoundName { get; set; } = "Group Stage";

        [Required(ErrorMessage = "Please select Team A")]
        [Display(Name = "Team A")]
        public int TeamAId { get; set; }

        [Required(ErrorMessage = "Please select Team B")]
        [Display(Name = "Team B")]
        public int TeamBId { get; set; }

        [Required(ErrorMessage = "Match date & time is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Match Date & Time")]
        public DateTime MatchDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Ground/Venue is required")]
        [StringLength(100)]
        [Display(Name = "Ground / Court / Field")]
        public string Ground { get; set; } = "Main Sports Complex";

        public IEnumerable<SelectListItem>? TournamentsList { get; set; }
        public IEnumerable<SelectListItem>? TeamsList { get; set; }
    }

    public class MatchScoreUpdateViewModel
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public string SportType { get; set; } = string.Empty;
        public string RoundName { get; set; } = string.Empty;

        public int TeamAId { get; set; }
        public string TeamAName { get; set; } = string.Empty;

        public int TeamBId { get; set; }
        public string TeamBName { get; set; } = string.Empty;

        [Display(Name = "Match Date & Time")]
        public DateTime MatchDate { get; set; }

        [Display(Name = "Ground")]
        public string Ground { get; set; } = string.Empty;

        [Display(Name = "Team A Score")]
        [StringLength(50)]
        public string? TeamAScore { get; set; }

        [Display(Name = "Team B Score")]
        [StringLength(50)]
        public string? TeamBScore { get; set; }

        [Display(Name = "Match Status")]
        public MatchStatus MatchStatus { get; set; }

        [Display(Name = "Winner Team")]
        public int? WinnerTeamId { get; set; }

        [Display(Name = "Match Highlights & Summary")]
        [StringLength(500)]
        public string? HighlightsSummary { get; set; }

        public IEnumerable<SelectListItem>? TeamsForWinnerList { get; set; }
    }
}
