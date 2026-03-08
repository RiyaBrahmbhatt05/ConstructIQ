using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.Data;
using ConstructionSimulator.Services;

namespace ConstructionSimulator.Controllers
{
    public class TasksController : Controller
    {
        private readonly InMemoryDataService _dataService;
        private readonly SimulationEngine _simulationEngine;

        public TasksController(InMemoryDataService dataService, SimulationEngine simulationEngine)
        {
            _dataService = dataService;
            _simulationEngine = simulationEngine;
        }

        // GET: Tasks/Create
        public IActionResult Create(int projectId)
        {
            ViewBag.ProjectID = projectId;
            ViewBag.Project = _dataService.GetProjectById(projectId);
            ViewBag.Crews = _dataService.GetAllCrews();
            ViewBag.Permits = _dataService.GetAllPermits();
            ViewBag.Tasks = _dataService.GetTasksByProjectId(projectId); // For dependencies

            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.Task task)
        {
            if (ModelState.IsValid)
            {
                // Run simulation before adding
                var simulationResult = _simulationEngine.SimulateTaskChange(task.ProjectID, task, isNewTask: true);

                // Check for critical conflicts
                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["ErrorMessage"] = simulationResult.Message;
                    ViewBag.Conflicts = simulationResult.Conflicts;
                    ViewBag.ProjectID = task.ProjectID;
                    ViewBag.Project = _dataService.GetProjectById(task.ProjectID);
                    ViewBag.Crews = _dataService.GetAllCrews();
                    ViewBag.Permits = _dataService.GetAllPermits();
                    ViewBag.Tasks = _dataService.GetTasksByProjectId(task.ProjectID);
                    return View(task);
                }

                _dataService.AddTask(task);

                // Log the simulation
                _dataService.AddSimulationLog(new Models.SimulationLog
                {
                    ProjectID = task.ProjectID,
                    TaskID = task.TaskID,
                    User = "Demo User",
                    ChangeType = "TaskAdded",
                    ChangeDetails = $"Added task: {task.Name}",
                    CostImpact = simulationResult.CostImpact,
                    ScheduleImpactDays = simulationResult.ScheduleImpactDays,
                    ImpactSummary = simulationResult.Message
                });

                TempData["SuccessMessage"] = $"Task '{task.Name}' added successfully!";
                if (simulationResult.Conflicts.Any())
                {
                    TempData["WarningMessage"] = simulationResult.Message;
                }

                return RedirectToAction("Details", "Projects", new { id = task.ProjectID });
            }

            ViewBag.ProjectID = task.ProjectID;
            ViewBag.Project = _dataService.GetProjectById(task.ProjectID);
            ViewBag.Crews = _dataService.GetAllCrews();
            ViewBag.Permits = _dataService.GetAllPermits();
            ViewBag.Tasks = _dataService.GetTasksByProjectId(task.ProjectID);
            return View(task);
        }

        // GET: Tasks/Edit/5
        public IActionResult Edit(int id)
        {
            var task = _dataService.GetTaskById(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Project = _dataService.GetProjectById(task.ProjectID);
            ViewBag.Crews = _dataService.GetAllCrews();
            ViewBag.Permits = _dataService.GetAllPermits();
            ViewBag.Tasks = _dataService.GetTasksByProjectId(task.ProjectID).Where(t => t.TaskID != id).ToList();

            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Models.Task task)
        {
            if (id != task.TaskID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Run simulation before updating
                var simulationResult = _simulationEngine.SimulateTaskChange(task.ProjectID, task, isNewTask: false);

                // Check for critical conflicts
                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["ErrorMessage"] = simulationResult.Message;
                    ViewBag.Conflicts = simulationResult.Conflicts;
                    ViewBag.Project = _dataService.GetProjectById(task.ProjectID);
                    ViewBag.Crews = _dataService.GetAllCrews();
                    ViewBag.Permits = _dataService.GetAllPermits();
                    ViewBag.Tasks = _dataService.GetTasksByProjectId(task.ProjectID).Where(t => t.TaskID != id).ToList();
                    return View(task);
                }

                _dataService.UpdateTask(task);

                // Log the simulation
                _dataService.AddSimulationLog(new Models.SimulationLog
                {
                    ProjectID = task.ProjectID,
                    TaskID = task.TaskID,
                    User = "Demo User",
                    ChangeType = "TaskModified",
                    ChangeDetails = $"Modified task: {task.Name}",
                    CostImpact = simulationResult.CostImpact,
                    ScheduleImpactDays = simulationResult.ScheduleImpactDays,
                    ImpactSummary = simulationResult.Message
                });

                TempData["SuccessMessage"] = $"Task '{task.Name}' updated successfully!";
                if (simulationResult.Conflicts.Any())
                {
                    TempData["WarningMessage"] = simulationResult.Message;
                }

                return RedirectToAction("Details", "Projects", new { id = task.ProjectID });
            }

            ViewBag.Project = _dataService.GetProjectById(task.ProjectID);
            ViewBag.Crews = _dataService.GetAllCrews();
            ViewBag.Permits = _dataService.GetAllPermits();
            ViewBag.Tasks = _dataService.GetTasksByProjectId(task.ProjectID).Where(t => t.TaskID != id).ToList();
            return View(task);
        }

        // GET: Tasks/Delete/5
        public IActionResult Delete(int id)
        {
            var task = _dataService.GetTaskById(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Project = _dataService.GetProjectById(task.ProjectID);
            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var task = _dataService.GetTaskById(id);
            if (task != null)
            {
                var projectId = task.ProjectID;
                _dataService.DeleteTask(id);
                TempData["SuccessMessage"] = $"Task '{task.Name}' deleted successfully!";
                return RedirectToAction("Details", "Projects", new { id = projectId });
            }
            return RedirectToAction("Index", "Projects");
        }
    }
}