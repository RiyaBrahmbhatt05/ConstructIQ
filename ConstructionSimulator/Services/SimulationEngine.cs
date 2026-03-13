using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Services
{
    public class SimulationEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly ConflictDetector _conflictDetector;

        public SimulationEngine(ApplicationDbContext context, ConflictDetector conflictDetector)
        {
            _context = context;
            _conflictDetector = conflictDetector;
        }

        public SimulationResultViewModel SimulateTaskChange(int projectId, ProjectTask modifiedTask, bool isNewTask = false)
        {
            var result = new SimulationResultViewModel { Success = true };

            // Get project from database
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null)
            {
                result.Success = false;
                result.Message = "Project not found";
                return result;
            }

            // Validate task dates
            if (modifiedTask.StartDate < project.StartDate || modifiedTask.EndDate > project.EndDate)
            {
                result.Conflicts.Add(new ConflictAlert
                {
                    Type = "Schedule",
                    Severity = "High",
                    Message = $"Task dates are outside project timeline ({project.StartDate:yyyy-MM-dd} to {project.EndDate:yyyy-MM-dd})",
                    RelatedTaskID = modifiedTask.ProjectTaskId,
                    RelatedTaskName = modifiedTask.Name
                });
            }

            // Check dependencies
            if (!string.IsNullOrEmpty(modifiedTask.Dependencies))
            {
                var dependencyIds = modifiedTask.Dependencies
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();

                foreach (var depId in dependencyIds)
                {
                    var dependentTask = _context.Tasks.FirstOrDefault(t => t.ProjectTaskId == depId);
                    if (dependentTask != null)
                    {
                        if (modifiedTask.StartDate < dependentTask.EndDate)
                        {
                            result.Conflicts.Add(new ConflictAlert
                            {
                                Type = "Schedule",
                                Severity = "Critical",
                                Message = $"Task cannot start before dependent task '{dependentTask.Name}' ends on {dependentTask.EndDate:yyyy-MM-dd}",
                                RelatedTaskID = modifiedTask.ProjectTaskId,
                                RelatedTaskName = modifiedTask.Name
                            });
                        }
                    }
                }
            }

            // Check crew availability
            if (modifiedTask.CrewId.HasValue)
            {
                var crewConflicts = _conflictDetector.DetectCrewConflicts(modifiedTask);
                result.Conflicts.AddRange(crewConflicts);
            }

            // Check permit requirements
            if (modifiedTask.RequiresPermit)
            {
                var permitConflicts = _conflictDetector.CheckPermitStatus(modifiedTask);
                result.Conflicts.AddRange(permitConflicts);
            }

            // Calculate cost impact
            var currentProjectCost = project.ActualCost;
            var projectedCost = currentProjectCost + modifiedTask.Cost;
            result.CostImpact = modifiedTask.Cost;

            if (projectedCost > project.Budget)
            {
                result.Conflicts.Add(new ConflictAlert
                {
                    Type = "Budget",
                    Severity = "Critical",
                    Message = $"Task will exceed project budget. Current: ${currentProjectCost:N2}, Projected: ${projectedCost:N2}, Budget: ${project.Budget:N2}",
                    RelatedTaskID = modifiedTask.ProjectTaskId,
                    RelatedTaskName = modifiedTask.Name
                });
            }

            // Calculate schedule impact
            var allTasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            if (!isNewTask)
            {
                allTasks = allTasks.Where(t => t.ProjectTaskId != modifiedTask.ProjectTaskId).ToList();
            }

            allTasks.Add(modifiedTask);

            var latestEndDate = allTasks.Max(t => t.EndDate);
            if (latestEndDate > project.EndDate)
            {
                result.ScheduleImpactDays = (latestEndDate - project.EndDate).Days;
                result.NewProjectEndDate = latestEndDate;
                result.Conflicts.Add(new ConflictAlert
                {
                    Type = "Schedule",
                    Severity = "High",
                    Message = $"Task will delay project completion by {result.ScheduleImpactDays} days (new end date: {latestEndDate:yyyy-MM-dd})",
                    RelatedTaskID = modifiedTask.ProjectTaskId,
                    RelatedTaskName = modifiedTask.Name
                });
            }

            // Generate summary message
            if (result.Conflicts.Count == 0)
            {
                result.Message = "No conflicts detected. Task can be safely added/modified.";
            }
            else
            {
                var criticalCount = result.Conflicts.Count(c => c.Severity == "Critical");
                var highCount = result.Conflicts.Count(c => c.Severity == "High");
                result.Message = $"Detected {result.Conflicts.Count} conflict(s): {criticalCount} Critical, {highCount} High priority.";
            }

            return result;
        }

        public SimulationResultViewModel SimulateProjectChange(Project modifiedProject)
        {
            var result = new SimulationResultViewModel { Success = true };

            var tasks = _context.Tasks
                .Where(t => t.ProjectId == modifiedProject.ProjectId)
                .ToList();

            // Check if any tasks fall outside new project dates
            var tasksOutsideRange = tasks
                .Where(t => t.StartDate < modifiedProject.StartDate || t.EndDate > modifiedProject.EndDate)
                .ToList();

            if (tasksOutsideRange.Any())
            {
                result.Conflicts.Add(new ConflictAlert
                {
                    Type = "Schedule",
                    Severity = "Critical",
                    Message = $"{tasksOutsideRange.Count} task(s) fall outside the new project timeline"
                });
            }

            // Check budget impact
            var totalTaskCost = tasks.Sum(t => t.Cost);
            if (totalTaskCost > modifiedProject.Budget)
            {
                result.Conflicts.Add(new ConflictAlert
                {
                    Type = "Budget",
                    Severity = "Critical",
                    Message = $"Current tasks cost (${totalTaskCost:N2}) exceeds new budget (${modifiedProject.Budget:N2})"
                });
            }

            result.Message = result.Conflicts.Count == 0
                ? "Project changes are compatible with existing tasks"
                : $"Detected {result.Conflicts.Count} conflict(s) with existing tasks";

            return result;
        }

        public List<ProjectTask> CalculateCriticalPath(int projectId)
        {
            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            return tasks
                .Where(t => t.Priority == "Critical" || t.Priority == "High")
                .OrderBy(t => t.EndDate)
                .ToList();
        }
    }
}