using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string FullName { get; set; }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}