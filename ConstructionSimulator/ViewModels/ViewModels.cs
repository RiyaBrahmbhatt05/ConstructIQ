namespace ConstructionSimulator.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalActualCost { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }

        public List<Models.Project> RecentProjects { get; set; } = new List<Models.Project>();
        public List<Models.Task> UpcomingTasks { get; set; } = new List<Models.Task>();
        public List<ConflictAlert> Conflicts { get; set; } = new List<ConflictAlert>();
    }

    public class ProjectDetailsViewModel
    {
        public Models.Project? Project { get; set; }
        public List<Models.Task> Tasks { get; set; } = new List<Models.Task>();
        public List<Models.Crew> AvailableCrews { get; set; } = new List<Models.Crew>();
        public List<Models.Permit> Permits { get; set; } = new List<Models.Permit>();
        public decimal TotalProjectCost { get; set; }
        public int CompletedTasksCount { get; set; }
        public int PendingTasksCount { get; set; }
        public List<ConflictAlert> ProjectConflicts { get; set; } = new List<ConflictAlert>();
    }

    public class ConflictAlert
    {
        public string Type { get; set; } = string.Empty; // Schedule, Resource, Budget, Permit
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public string Message { get; set; } = string.Empty;
        public DateTime DetectedDate { get; set; } = DateTime.Now;
        public int? RelatedTaskID { get; set; }
        public string? RelatedTaskName { get; set; }
    }

    public class SimulationResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ConflictAlert> Conflicts { get; set; } = new List<ConflictAlert>();
        public decimal? CostImpact { get; set; }
        public int? ScheduleImpactDays { get; set; }
        public DateTime? NewProjectEndDate { get; set; }
    }
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}