using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class Project
    {
        public int ProjectID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        public decimal ActualCost { get; set; }

        public string? Description { get; set; }

        public string Status { get; set; } = "Planning"; // Planning, In Progress, Completed, On Hold

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public List<Task> Tasks { get; set; } = new List<Task>();

        // Calculated properties
        public int TotalDays => (EndDate - StartDate).Days;
        public decimal BudgetUtilization => Budget > 0 ? (ActualCost / Budget) * 100 : 0;
        public bool IsOverBudget => ActualCost > Budget;
    }
}
