using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class SimulationLog
    {
        public int SimulationLogId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public int? ProjectTaskId { get; set; }

        [Required]
        public string User { get; set; } = "System";

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        public string ChangeType { get; set; } = string.Empty; // TaskAdded, TaskModified, CrewAssigned, etc.

        public string? ChangeDetails { get; set; }

        public string? ImpactSummary { get; set; }

        public decimal? CostImpact { get; set; }

        public int? ScheduleImpactDays { get; set; }

        // Navigation property
        public Project? Project { get; set; }
    }
}

