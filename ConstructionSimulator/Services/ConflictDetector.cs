using ConstructionSimulator.Data;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Services
{
    public class ConflictDetector
    {
        private readonly InMemoryDataService _dataService;

        public ConflictDetector(InMemoryDataService dataService)
        {
            _dataService = dataService;
        }

        public List<ConflictAlert> DetectAllConflicts(int projectId)
        {
            var conflicts = new List<ConflictAlert>();

            var tasks = _dataService.GetTasksByProjectId(projectId);

            // Check for crew conflicts
            foreach (var task in tasks.Where(t => t.CrewID.HasValue && t.Status == "In Progress"))
            {
                conflicts.AddRange(DetectCrewConflicts(task));
            }

            // Check for permit issues
            foreach (var task in tasks.Where(t => t.RequiresPermit))
            {
                conflicts.AddRange(CheckPermitStatus(task));
            }

            // Check for schedule conflicts
            conflicts.AddRange(DetectScheduleConflicts(projectId));

            // Check for budget issues
            conflicts.AddRange(DetectBudgetConflicts(projectId));

            return conflicts;
        }

        public List<ConflictAlert> DetectCrewConflicts(Models.Task task)
        {
            var conflicts = new List<ConflictAlert>();

            if (!task.CrewID.HasValue) return conflicts;

            var crew = _dataService.GetCrewById(task.CrewID.Value);
            if (crew == null) return conflicts;

            // Check if crew is available
            if (!crew.IsAvailable)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Resource",
                    Severity = "High",
                    Message = $"Crew '{crew.Name}' is currently unavailable",
                    RelatedTaskID = task.TaskID,
                    RelatedTaskName = task.Name
                });
            }

            // Check for overlapping tasks with same crew
            var allTasks = _dataService.GetAllTasks();
            var overlappingTasks = allTasks.Where(t =>
                t.TaskID != task.TaskID &&
                t.CrewID == task.CrewID &&
                t.Status != "Completed" &&
                ((t.StartDate <= task.EndDate && t.EndDate >= task.StartDate))
            ).ToList();

            foreach (var overlap in overlappingTasks)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Resource",
                    Severity = "Critical",
                    Message = $"Crew '{crew.Name}' is already assigned to task '{overlap.Name}' during this period ({overlap.StartDate:yyyy-MM-dd} to {overlap.EndDate:yyyy-MM-dd})",
                    RelatedTaskID = task.TaskID,
                    RelatedTaskName = task.Name
                });
            }

            return conflicts;
        }

        public List<ConflictAlert> CheckPermitStatus(Models.Task task)
        {
            var conflicts = new List<ConflictAlert>();

            if (!task.RequiresPermit) return conflicts;

            if (!task.PermitID.HasValue)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Permit",
                    Severity = "Critical",
                    Message = "Task requires a permit but none is assigned",
                    RelatedTaskID = task.TaskID,
                    RelatedTaskName = task.Name
                });
                return conflicts;
            }

            var permit = _dataService.GetPermitById(task.PermitID.Value);
            if (permit == null)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Permit",
                    Severity = "Critical",
                    Message = "Assigned permit not found",
                    RelatedTaskID = task.TaskID,
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
                    RelatedTaskID = task.TaskID,
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
                    RelatedTaskID = task.TaskID,
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
                    RelatedTaskID = task.TaskID,
                    RelatedTaskName = task.Name
                });
            }

            return conflicts;
        }

        public List<ConflictAlert> DetectScheduleConflicts(int projectId)
        {
            var conflicts = new List<ConflictAlert>();
            var tasks = _dataService.GetTasksByProjectId(projectId);

            // Check for tasks that violate dependencies
            foreach (var task in tasks)
            {
                if (string.IsNullOrEmpty(task.Dependencies)) continue;

                var dependencyIds = task.Dependencies.Split(',').Select(int.Parse).ToList();
                foreach (var depId in dependencyIds)
                {
                    var dependentTask = _dataService.GetTaskById(depId);
                    if (dependentTask != null && task.StartDate < dependentTask.EndDate)
                    {
                        conflicts.Add(new ConflictAlert
                        {
                            Type = "Schedule",
                            Severity = "Critical",
                            Message = $"Task '{task.Name}' starts before its dependency '{dependentTask.Name}' ends",
                            RelatedTaskID = task.TaskID,
                            RelatedTaskName = task.Name
                        });
                    }
                }
            }

            // Check for overdue tasks
            var overdueTasks = tasks.Where(t => t.IsDelayed).ToList();
            foreach (var task in overdueTasks)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Schedule",
                    Severity = "High",
                    Message = $"Task '{task.Name}' is overdue (expected completion: {task.EndDate:yyyy-MM-dd})",
                    RelatedTaskID = task.TaskID,
                    RelatedTaskName = task.Name
                });
            }

            return conflicts;
        }

        public List<ConflictAlert> DetectBudgetConflicts(int projectId)
        {
            var conflicts = new List<ConflictAlert>();
            var project = _dataService.GetProjectById(projectId);

            if (project == null) return conflicts;

            if (project.IsOverBudget)
            {
                var overrun = project.ActualCost - project.Budget;
                conflicts.Add(new ConflictAlert
                {
                    Type = "Budget",
                    Severity = "Critical",
                    Message = $"Project is over budget by ${overrun:N2} ({project.BudgetUtilization:F1}% utilized)",
                });
            }
            else if (project.BudgetUtilization > 90)
            {
                conflicts.Add(new ConflictAlert
                {
                    Type = "Budget",
                    Severity = "High",
                    Message = $"Project budget is {project.BudgetUtilization:F1}% utilized - approaching limit",
                });
            }

            return conflicts;
        }
    }
}