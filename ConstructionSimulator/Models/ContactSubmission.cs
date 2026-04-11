using System;
using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class ContactSubmission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Subject { get; set; }

        public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
