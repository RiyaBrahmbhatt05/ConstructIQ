using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class Permit
    {
        public int PermitID { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty; // Building, Electrical, Plumbing, etc.

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Expired

        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        public DateTime? ApprovalDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Fee { get; set; }

        public string IssuingAuthority { get; set; } = string.Empty;

        public string? Notes { get; set; }

        // Navigation property
        public List<Task> RelatedTasks { get; set; } = new List<Task>();

        // Calculated properties
        public bool IsApproved => Status == "Approved";
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
        public int ProcessingDays => ApprovalDate.HasValue
            ? (ApprovalDate.Value - ApplicationDate).Days
            : (DateTime.Now - ApplicationDate).Days;
    }
}

