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
            ViewBag.ProjectId = projectId;
            ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            ViewBag.Crews = _context.Crews.ToList();
            ViewBag.Permits = _context.Permits.ToList();
            ViewBag.Tasks = _context.Tasks.Where(t => t.ProjectId == projectId).ToList();
            ViewBag.MaterialsList = _context.Materials.ToList();

            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProjectTask task, int[] materialIds, decimal[] unitsRequired)
        {
            if (ModelState.IsValid)
            {
                var simulationResult = _simulationEngine.SimulateTaskChange(task.ProjectId, task, isNewTask: true);

                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["ErrorMessage"] = simulationResult.Message;
                    ViewBag.Conflicts = simulationResult.Conflicts;
                    ViewBag.ProjectId = task.ProjectId;
                    ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
                    ViewBag.Crews = _context.Crews.ToList();
                    ViewBag.Permits = _context.Permits.ToList();
                    ViewBag.Tasks = _context.Tasks.Where(t => t.ProjectId == task.ProjectId).ToList();
                    ViewBag.MaterialsList = _context.Materials.ToList();
                    return View(task);
                }

                // Save task first
                _context.Tasks.Add(task);
                _context.SaveChanges();

                // Save selected materials
                SaveTaskMaterials(task.ProjectTaskId, materialIds, unitsRequired);

                // Auto-calculate and save task cost
                _costCalculator.RecalculateAndSaveTaskCost(task.ProjectTaskId);

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

            ViewBag.ProjectId = task.ProjectId;
            ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
            ViewBag.Crews = _context.Crews.ToList();
            ViewBag.Permits = _context.Permits.ToList();
            ViewBag.Tasks = _context.Tasks.Where(t => t.ProjectId == task.ProjectId).ToList();
            ViewBag.MaterialsList = _context.Materials.ToList();
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

            ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
            ViewBag.Crews = _context.Crews.ToList();
            ViewBag.Permits = _context.Permits.ToList();
            ViewBag.Tasks = _context.Tasks
                .Where(t => t.ProjectId == task.ProjectId && t.ProjectTaskId != id)
                .ToList();
            ViewBag.MaterialsList = _context.Materials.ToList();

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
                var simulationResult = _simulationEngine.SimulateTaskChange(task.ProjectId, task, isNewTask: false);

                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["ErrorMessage"] = simulationResult.Message;
                    ViewBag.Conflicts = simulationResult.Conflicts;
                    ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
                    ViewBag.Crews = _context.Crews.ToList();
                    ViewBag.Permits = _context.Permits.ToList();
                    ViewBag.Tasks = _context.Tasks
                        .Where(t => t.ProjectId == task.ProjectId && t.ProjectTaskId != id)
                        .ToList();
                    ViewBag.MaterialsList = _context.Materials.ToList();
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

            ViewBag.Project = _context.Projects.FirstOrDefault(p => p.ProjectId == task.ProjectId);
            ViewBag.Crews = _context.Crews.ToList();
            ViewBag.Permits = _context.Permits.ToList();
            ViewBag.Tasks = _context.Tasks
                .Where(t => t.ProjectId == task.ProjectId && t.ProjectTaskId != id)
                .ToList();
            ViewBag.MaterialsList = _context.Materials.ToList();

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

                TempData["SuccessMessage"] = $"Task '{task.Name}' deleted successfully!";
                return RedirectToAction("Details", "Projects", new { id = projectId });
            }

            return RedirectToAction("Index", "Projects");
        }

        private void SaveTaskMaterials(int projectTaskId, int[] materialIds, decimal[] unitsRequired)
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
    }
}