using System.ComponentModel.DataAnnotations;

namespace KhelTrack.Models.ViewModels
{
    public class PlayerInputModel
    {
        [Required(ErrorMessage = "Player name is required")]
        [StringLength(100)]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Roll number is required")]
        [StringLength(50)]
        [Display(Name = "Roll Number")]
        public string RollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Range(0, 999)]
        [Display(Name = "Jersey #")]
        public int? JerseyNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Position / Role")]
        public string? Position { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact")]
        public string? ContactNumber { get; set; }
    }

    public class TeamRegistrationViewModel
    {
        [Required(ErrorMessage = "Please select a tournament")]
        [Display(Name = "Tournament")]
        public int TournamentId { get; set; }

        [Required(ErrorMessage = "Team name is required")]
        [StringLength(100)]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        [Display(Name = "Department / Branch")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Captain contact phone is required")]
        [StringLength(20)]
        [Display(Name = "Captain Phone Number")]
        public string ContactPhone { get; set; } = string.Empty;

        // Dynamic player roster
        public List<PlayerInputModel> Players { get; set; } = new List<PlayerInputModel>();
    }

    public class AddPlayerViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Player name is required")]
        [StringLength(100)]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Roll number is required")]
        [StringLength(50)]
        public string RollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(0, 999)]
        public int? JerseyNumber { get; set; }

        [StringLength(50)]
        public string? Position { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }
    }
}
