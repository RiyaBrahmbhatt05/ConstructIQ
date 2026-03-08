using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class Crew
    {
        public int CrewID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SkillType { get; set; } = string.Empty; // Carpentry, Electrical, Plumbing, etc.

        [Range(1, 100)]
        public int TeamSize { get; set; } = 1;

        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        public bool IsAvailable { get; set; } = true;

        public string? Notes { get; set; }

        // Navigation property
        public List<Task> Tasks { get; set; } = new List<Task>();

        // Calculated properties
        public int ActiveTasksCount => Tasks?.Count(t => t.Status == "In Progress") ?? 0;
        public decimal DailyCost => HourlyRate * 8 * TeamSize; // Assuming 8-hour workday
    }
}