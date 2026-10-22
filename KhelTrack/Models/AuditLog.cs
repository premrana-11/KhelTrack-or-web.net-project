using System.ComponentModel.DataAnnotations;

namespace KhelTrack.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [StringLength(100)]
        [Display(Name = "User ID")]
        public string? UserId { get; set; }

        [StringLength(100)]
        [Display(Name = "User Email")]
        public string? UserEmail { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty; // e.g., TEAM_REGISTERED, SCORE_UPDATED, STATUS_APPROVED

        [Required]
        [StringLength(50)]
        [Display(Name = "Entity Type")]
        public string EntityType { get; set; } = string.Empty; // e.g., Tournament, Team, MatchFixture, User

        [Required]
        [StringLength(50)]
        [Display(Name = "Entity ID")]
        public string EntityId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Details")]
        public string Details { get; set; } = string.Empty;

        [Display(Name = "Timestamp (UTC)")]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }
    }
}
