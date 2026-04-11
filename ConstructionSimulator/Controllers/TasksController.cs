using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.Data;
using ConstructionSimulator.Services;
using ConstructionSimulator.Models;

namespace ConstructionSimulator.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SimulationEngine _simulationEngine;
        private readonly CostCalculator _costCalculator;

        public TasksController(
            ApplicationDbContext context,
            SimulationEngine simulationEngine,
            CostCalculator costCalculator)
        {
            _context = context;
            _simulationEngine = simulationEngine;
            _costCalculator = costCalculator;
        }

        // GET: Tasks/Create
        public IActionResult Create(int projectId)
        {
            if (projectId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a project first before creating a task.";
                return RedirectToAction("Index", "Projects");
            }

            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null)
            {
                TempData["ErrorMessage"] = "Project not found.";
                return RedirectToAction("Index", "Projects");
            }

            PopulateTaskViewBags(projectId);

            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProjectTask task, int[]? materialIds, decimal[]? unitsRequired)
        {
            if (task.ProjectId <= 0)
            {
                ModelState.AddModelError(nameof(task.ProjectId), "Project is required.");
            }

            if (ModelState.IsValid)
            {
                if (!ValidateDependencies(task))
                {
                    PopulateTaskViewBags(task.ProjectId);
                    return View(task);
                }

                var simulationResult = _simulationEngine.SimulateTaskChange(task.ProjectId, task, isNewTask: true);

                if (!simulationResult.Success)
                {
                    TempData["ErrorMessage"] = simulationResult.Message;
                    ModelState.AddModelError(string.Empty, simulationResult.Message);
                    PopulateTaskViewBags(task.ProjectId);
                    return View(task);
                }

                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["ErrorMessage"] = simulationResult.Message;
                    ModelState.AddModelError(string.Empty, simulationResult.Message);
                    ViewBag.Conflicts = simulationResult.Conflicts;
                    PopulateTaskViewBags(task.ProjectId);
                    return View(task);
                }

                // Save task first
                _context.Tasks.Add(task);
                _context.SaveChanges();

                // Save selected materials
                SaveTaskMaterials(task.ProjectTaskId, materialIds, unitsRequired);

                // Auto-calculate and save task cost
                _costCalculator.RecalculateAndSaveTaskCost(task.ProjectTaskId);
                _costCalculator.RecalculateAndSaveProjectCost(task.ProjectId);

                _context.SimulationLogs.Add(new SimulationLog
                {
                    ProjectId = task.ProjectId,
                    ProjectTaskId = task.ProjectTaskId,
                    User = "Demo User",
                    ChangeType = "TaskAdded",
                    ChangeDetails = $"Added task: {task.Name}",
                    CostImpact = task.Cost,
                    ScheduleImpactDays = simulationResult.ScheduleImpactDays,
                    ImpactSummary = simulationResult.Message
                });

                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Task '{task.Name}' added successfully!";
                if (simulationResult.Conflicts.Any())
                {
                    TempData["WarningMessage"] = simulationResult.Message;
                }

                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            PopulateTaskViewBags(task.ProjectId);
            return View(task);
        }

        // GET: Tasks/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.ProjectTaskId == id);
            if (task == null)
            {
                return NotFound();
            }

            PopulateTaskViewBags(task.ProjectId, id);

            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProjectTask task, int[] materialIds, decimal[] unitsRequired)
        {
            if (id != task.ProjectTaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!ValidateDependencies(task, id))
                {
                    PopulateTaskViewBags(task.ProjectId, id);
                    return View(task);
                }

                var simulationResult = _simulationEngine.SimulateTaskChange(task.ProjectId, task, isNewTask: false);

                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["ErrorMessage"] = simulationResult.Message;
                    ModelState.AddModelError(string.Empty, simulationResult.Message);
                    ViewBag.Conflicts = simulationResult.Conflicts;
                    PopulateTaskViewBags(task.ProjectId, id);
                    return View(task);
                }

                var existingTask = _context.Tasks.FirstOrDefault(t => t.ProjectTaskId == id);
                if (existingTask == null)
                {
                    return NotFound();
                }

                existingTask.Name = task.Name;
                existingTask.Description = task.Description;
                existingTask.StartDate = task.StartDate;
                existingTask.EndDate = task.EndDate;
                existingTask.Duration = task.Duration;
                existingTask.Cost = task.Cost;
                existingTask.CrewId = task.CrewId;
                existingTask.Status = task.Status;
                existingTask.Priority = task.Priority;
                existingTask.Dependencies = task.Dependencies;
                existingTask.PermitId = task.PermitId;
                existingTask.RequiresPermit = task.RequiresPermit;

                _context.SaveChanges();

                // Replace task materials
                var oldTaskMaterials = _context.TaskMaterials
                    .Where(tm => tm.ProjectTaskId == existingTask.ProjectTaskId)
                    .ToList();

                if (oldTaskMaterials.Any())
                {
                    _context.TaskMaterials.RemoveRange(oldTaskMaterials);
                    _context.SaveChanges();
                }

                SaveTaskMaterials(existingTask.ProjectTaskId, materialIds, unitsRequired);

                // Auto-calculate and save task cost
                _costCalculator.RecalculateAndSaveTaskCost(existingTask.ProjectTaskId);
                _costCalculator.RecalculateAndSaveProjectCost(existingTask.ProjectId);

                _context.SimulationLogs.Add(new SimulationLog
                {
                    ProjectId = existingTask.ProjectId,
                    ProjectTaskId = existingTask.ProjectTaskId,
                    User = "Demo User",
                    ChangeType = "TaskModified",
                    ChangeDetails = $"Modified task: {existingTask.Name}",
                    CostImpact = existingTask.Cost,
                    ScheduleImpactDays = simulationResult.ScheduleImpactDays,
                    ImpactSummary = simulationResult.Message
                });

                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Task '{existingTask.Name}' updated successfully!";
                if (simulationResult.Conflicts.Any())
                {
                    TempData["WarningMessage"] = simulationResult.Message;
                }

                return RedirectToAction("Details", "Projects", new { id = existingTask.ProjectId });
            }

            PopulateTaskViewBags(task.ProjectId, id);

            return View(task);
        }

        // GET: Tasks/Delete/5
        public IActionResult Delete(int id)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.ProjectTaskId == id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int ProjectTaskId)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.ProjectTaskId == ProjectTaskId);
            if (task != null)
            {
                var projectId = task.ProjectId;

                var relatedLogs = _context.SimulationLogs
                    .Where(l => l.ProjectTaskId == ProjectTaskId)
                    .ToList();

                var relatedTaskMaterials = _context.TaskMaterials
                    .Where(tm => tm.ProjectTaskId == ProjectTaskId)
                    .ToList();

                if (relatedLogs.Any())
                {
                    _context.SimulationLogs.RemoveRange(relatedLogs);
                }

                if (relatedTaskMaterials.Any())
                {
                    _context.TaskMaterials.RemoveRange(relatedTaskMaterials);
                }

                _context.Tasks.Remove(task);
                _context.SaveChanges();
                _costCalculator.RecalculateAndSaveProjectCost(projectId);

                TempData["SuccessMessage"] = $"Task '{task.Name}' deleted successfully!";
                return RedirectToAction("Details", "Projects", new { id = projectId });
            }

            return RedirectToAction("Index", "Projects");
        }

        private void SaveTaskMaterials(int projectTaskId, int[]? materialIds, decimal[]? unitsRequired)
        {
            if (materialIds == null || unitsRequired == null)
                return;

            var count = Math.Min(materialIds.Length, unitsRequired.Length);

            for (int i = 0; i < count; i++)
            {
                if (materialIds[i] > 0 && unitsRequired[i] > 0)
                {
                    var material = _context.Materials.FirstOrDefault(m => m.MaterialId == materialIds[i]);
                    if (material != null)
                    {
                        var subtotal = material.CostPerUnit * unitsRequired[i];

                        _context.TaskMaterials.Add(new TaskMaterial
                        {
                            ProjectTaskId = projectTaskId,
                            MaterialId = materialIds[i],
                            UnitsRequired = unitsRequired[i],
                            SubtotalCost = subtotal
                        });
                    }
                }
            }

            _context.SaveChanges();
        }

        private void PopulateTaskViewBags(int projectId, int? excludeTaskId = null)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            ViewBag.Crews = _context.Crews.ToList();
            ViewBag.Permits = _context.Permits.ToList();

            var taskQuery = _context.Tasks.Where(t => t.ProjectId == projectId);
            if (excludeTaskId.HasValue)
            {
                taskQuery = taskQuery.Where(t => t.ProjectTaskId != excludeTaskId.Value);
            }

            ViewBag.Tasks = taskQuery.OrderBy(t => t.ProjectTaskId).ToList();
            ViewBag.MaterialsList = _context.Materials.ToList();
        }

        private bool ValidateDependencies(ProjectTask task, int? excludeTaskId = null)
        {
            if (string.IsNullOrWhiteSpace(task.Dependencies))
            {
                return true;
            }

            var rawValues = task.Dependencies
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var dependencyIds = new List<int>();
            var invalidValues = new List<string>();

            foreach (var value in rawValues)
            {
                if (int.TryParse(value, out var dependencyId))
                {
                    if (!dependencyIds.Contains(dependencyId))
                    {
                        dependencyIds.Add(dependencyId);
                    }
                }
                else
                {
                    invalidValues.Add(value);
                }
            }

            if (invalidValues.Any())
            {
                ModelState.AddModelError(nameof(task.Dependencies), $"Invalid task ID(s): {string.Join(", ", invalidValues)}.");
            }

            if (excludeTaskId.HasValue && dependencyIds.Contains(excludeTaskId.Value))
            {
                ModelState.AddModelError(nameof(task.Dependencies), "A task cannot depend on itself.");
            }

            if (!dependencyIds.Any())
            {
                return ModelState.IsValid;
            }

            var dependencyTasks = _context.Tasks
                .Where(t => t.ProjectId == task.ProjectId && dependencyIds.Contains(t.ProjectTaskId))
                .ToList();

            var missingIds = dependencyIds.Except(dependencyTasks.Select(t => t.ProjectTaskId)).ToList();
            if (missingIds.Any())
            {
                ModelState.AddModelError(nameof(task.Dependencies), $"Unknown dependency task ID(s): {string.Join(", ", missingIds)}.");
            }

            foreach (var dependencyTask in dependencyTasks)
            {
                if (dependencyTask.EndDate.Date >= task.StartDate.Date)
                {
                    ModelState.AddModelError(nameof(task.StartDate),
                        $"Task '{task.Name}' must start after dependency task '{dependencyTask.Name}' finishes on {dependencyTask.EndDate:MMM dd, yyyy}.");
                }
            }

            return ModelState.IsValid;
        }
    }
}