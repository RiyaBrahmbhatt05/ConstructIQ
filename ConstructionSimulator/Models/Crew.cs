using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class Crew
    {
        public int CrewId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SkillType { get; set; } = string.Empty;

        [Range(1, 100)]
        public int TeamSize { get; set; } = 1;

        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(200)]
        public string? RequiredLicenses { get; set; }

        public string? MemberNames { get; set; }

        public string? ContactDetails { get; set; }

        [Range(0, 100)]
        public int YearsOfExperience { get; set; }

        public string? Notes { get; set; }

        public List<ProjectTask> Tasks { get; set; } = new();

        public int ActiveTasksCount => Tasks?.Count(t => t.Status == "In Progress") ?? 0;
        public decimal DailyCost => HourlyRate * 8;
    }
}