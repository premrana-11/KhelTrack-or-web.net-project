using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace KhelTrack.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "College Roll Number")]
        public string CollegeRollNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Team> CaptainedTeams { get; set; } = new List<Team>();
    }
}
