using System.ComponentModel.DataAnnotations;

namespace KhelTrack.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tournament name is required")]
        [StringLength(150, ErrorMessage = "Tournament name cannot exceed 150 characters")]
        [Display(Name = "Tournament Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sport type is required")]
        [StringLength(50)]
        [Display(Name = "Sport Category")]
        public string SportType { get; set; } = string.Empty; // e.g., Cricket, Football, Badminton, Basketball, Volleyball

        [StringLength(500)]
        [Display(Name = "Short Description")]
        public string? Description { get; set; }

        [Display(Name = "Rules & Guidelines")]
        public string? Rules { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [StringLength(100)]
        [Display(Name = "Campus Venue / Ground")]
        public string Venue { get; set; } = string.Empty;

        [Range(2, 64, ErrorMessage = "Max teams must be between 2 and 64")]
        [Display(Name = "Maximum Teams")]
        public int MaxTeams { get; set; } = 16;

        [Display(Name = "Tournament Status")]
        public TournamentStatus Status { get; set; } = TournamentStatus.Upcoming;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<MatchFixture> Matches { get; set; } = new List<MatchFixture>();
    }
}
