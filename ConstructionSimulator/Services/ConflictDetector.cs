using ConstructionSimulator.Data;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Services
{
    public class ConflictDetector
    {
        private readonly ApplicationDbContext _context;

        public ConflictDetector(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ConflictAlert> DetectAllConflicts(int projectId)
        {
            var conflicts = new List<ConflictAlert>();

            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            foreach (var task in tasks.Where(t => t.CrewId.HasValue && t.Status == "In Progress"))
            {
                conflicts.AddRange(DetectCrewConflicts(task));
            }

            foreach (var task in tasks.Where(t => t.RequiresPermit))
            {
                conflicts.AddRange(CheckPermitStatus(task));
            }

            conflicts.AddRange(DetectScheduleConflicts(projectId));
            conflicts.AddRange(DetectBudgetConflicts(projectId));

            return conflicts;
        }

        public List<ConflictAlert> DetectCrewConflicts(Models.ProjectTask task)
        {
            var conflicts = new List<ConflictAlert>();

            if (!task.CrewId.HasValue) return conflicts;

            var crew = _context.Crews.FirstOrDefault(c => c.CrewId == task.CrewId.Value);
            if (crew == null) return conflicts;

            if (!crew.IsAvailable)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Resource",
                    Severity = "High",
                    Message = $"Crew '{crew.Name}' is currently unavailable",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
            }

            var allTasks = _context.Tasks.ToList();
            var overlappingTasks = allTasks.Where(t =>
                t.ProjectTaskId != task.ProjectTaskId &&
                t.CrewId == task.CrewId &&
                t.Status != "Completed" &&
                t.StartDate <= task.EndDate &&
                t.EndDate >= task.StartDate
            ).ToList();

            foreach (var overlap in overlappingTasks)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Resource",
                    Severity = "Critical",
                    Message = $"Crew '{crew.Name}' is already assigned to task '{overlap.Name}' during this period ({overlap.StartDate:yyyy-MM-dd} to {overlap.EndDate:yyyy-MM-dd})",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
            }

            return conflicts;
        }

        public List<ConflictAlert> CheckPermitStatus(Models.ProjectTask task)
        {
            var conflicts = new List<ConflictAlert>();

            if (!task.RequiresPermit) return conflicts;

            if (!task.PermitId.HasValue)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Permit",
                    Severity = "Critical",
                    Message = "Task requires a permit but none is assigned",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
                return conflicts;
            }

            var permit = _context.Permits.FirstOrDefault(p => p.PermitId == task.PermitId.Value);
            if (permit == null)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Permit",
                    Severity = "Critical",
                    Message = "Assigned permit not found",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
                return conflicts;
            }

            if (permit.Status == "Pending")
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Permit",
                    Severity = "High",
                    Message = $"Permit '{permit.Type}' is still pending approval",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
            }
            else if (permit.Status == "Rejected")
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Permit",
                    Severity = "Critical",
                    Message = $"Permit '{permit.Type}' has been rejected",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
            }
            else if (permit.IsExpired)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Permit",
                    Severity = "Critical",
                    Message = $"Permit '{permit.Type}' has expired on {permit.ExpiryDate:yyyy-MM-dd}",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
            }

            return conflicts;
        }

        public List<ConflictAlert> DetectScheduleConflicts(int projectId)
        {
            var conflicts = new List<ConflictAlert>();
            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            foreach (var task in tasks)
            {
                if (string.IsNullOrEmpty(task.Dependencies)) continue;

                var dependencyIds = task.Dependencies
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();

                foreach (var depId in dependencyIds)
                {
                    var dependentTask = _context.Tasks.FirstOrDefault(t => t.ProjectTaskId == depId);
                    if (dependentTask != null && task.StartDate < dependentTask.EndDate)
                    {
                        conflicts.Add(new ConflictAlert
                        {
                            Type = "Schedule",
                            Severity = "Critical",
                            Message = $"Task '{task.Name}' starts before its dependency '{dependentTask.Name}' ends",
                            RelatedTaskID = task.ProjectTaskId,
                            RelatedTaskName = task.Name
                        });
                    }
                }
            }

            var overdueTasks = tasks.Where(t => t.IsDelayed).ToList();
            foreach (var task in overdueTasks)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Schedule",
                    Severity = "High",
                    Message = $"Task '{task.Name}' is overdue (expected completion: {task.EndDate:yyyy-MM-dd})",
                    RelatedTaskID = task.ProjectTaskId,
                    RelatedTaskName = task.Name
                });
            }

            return conflicts;
        }

        public List<ConflictAlert> DetectBudgetConflicts(int projectId)
        {
            var conflicts = new List<ConflictAlert>();
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);

            if (project == null) return conflicts;

            if (project.IsOverBudget)
            {
                var overrun = project.ActualCost - project.Budget;
                conflicts.Add(new ConflictAlert
                {
                    Type = "Budget",
                    Severity = "Critical",
                    Message = $"Project is over budget by ${overrun:N2} ({project.BudgetUtilization:F1}% utilized)"
                });
            }
            else if (project.BudgetUtilization > 90)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Budget",
                    Severity = "High",
                    Message = $"Project budget is {project.BudgetUtilization:F1}% utilized - approaching limit"
                });
            }

            return conflicts;
        }
    }
}