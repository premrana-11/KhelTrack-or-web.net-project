using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhelTrack.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }

        [Required(ErrorMessage = "Player/Student name is required")]
        [StringLength(100)]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Roll number is required")]
        [StringLength(50)]
        [Display(Name = "College Roll Number")]
        public string RollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Range(0, 999, ErrorMessage = "Jersey number must be between 0 and 999")]
        [Display(Name = "Jersey #")]
        public int? JerseyNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Position / Role")]
        public string? Position { get; set; } // e.g., Striker, Goalkeeper, Batsman, Bowler, Point Guard, Raider

        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }
    }
}
