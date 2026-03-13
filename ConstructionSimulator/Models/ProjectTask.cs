using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class ProjectTask
    {
        public int ProjectTaskId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Duration { get; set; } // in days

        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        public int? CrewId { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Blocked

        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        // Dependencies (comma-separated TaskId)
        public string? Dependencies { get; set; }

        public int? PermitId { get; set; }

        public bool RequiresPermit { get; set; } = false;

        // Navigation properties
        public Project? Project { get; set; }
        public Crew? Crew { get; set; }
        public Permit? Permit { get; set; }

        // Calculated properties
        public int TotalDays => (EndDate - StartDate).Days;
        public bool IsDelayed => DateTime.Now > EndDate && Status != "Completed";
        public bool IsCritical => Priority == "Critical" || Priority == "High";
    }
}

