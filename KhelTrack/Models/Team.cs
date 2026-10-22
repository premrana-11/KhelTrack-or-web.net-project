using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhelTrack.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters")]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Captain")]
        public string CaptainId { get; set; } = string.Empty;

        [ForeignKey("CaptainId")]
        public virtual ApplicationUser? Captain { get; set; }

        [Required]
        [Display(Name = "Tournament")]
        public int TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public virtual Tournament? Tournament { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        [Display(Name = "Department / Branch")]
        public string Department { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [Display(Name = "Registration Status")]
        public TeamStatus RegistrationStatus { get; set; } = TeamStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Reviewer Comments / Rejection Reason")]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();

        [InverseProperty("TeamA")]
        public virtual ICollection<MatchFixture> HomeMatches { get; set; } = new List<MatchFixture>();

        [InverseProperty("TeamB")]
        public virtual ICollection<MatchFixture> AwayMatches { get; set; } = new List<MatchFixture>();
    }
}
