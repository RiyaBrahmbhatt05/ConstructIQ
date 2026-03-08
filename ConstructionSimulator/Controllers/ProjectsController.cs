using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using ConstructionSimulator.Services;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly InMemoryDataService _dataService;
        private readonly SimulationEngine _simulationEngine;
        private readonly ConflictDetector _conflictDetector;

        public ProjectsController(InMemoryDataService dataService, SimulationEngine simulationEngine, ConflictDetector conflictDetector)
        {
            _dataService = dataService;
            _simulationEngine = simulationEngine;
            _conflictDetector = conflictDetector;
        }

        // GET: Projects
        public IActionResult Index()
        {
            var projects = _dataService.GetAllProjects();
            return View(projects);
        }

        // GET: Projects/Details/5
        public IActionResult Details(int id)
        {
            var project = _dataService.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Tasks = _dataService.GetTasksByProjectId(id),
                AvailableCrews = _dataService.GetAllCrews(),
                Permits = _dataService.GetAllPermits(),
                ProjectConflicts = _conflictDetector.DetectAllConflicts(id)
            };

            viewModel.CompletedTasksCount = viewModel.Tasks.Count(t => t.Status == "Completed");
            viewModel.PendingTasksCount = viewModel.Tasks.Count(t => t.Status == "Pending");
            viewModel.TotalProjectCost = viewModel.Tasks.Sum(t => t.Cost);

            return View(viewModel);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Project project)
        {
            if (ModelState.IsValid)
            {
                project.CreatedDate = DateTime.Now;
                project.ActualCost = 0;
                _dataService.AddProject(project);

                TempData["SuccessMessage"] = $"Project '{project.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public IActionResult Edit(int id)
        {
            var project = _dataService.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Project project)
        {
            if (id != project.ProjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Run simulation to check for conflicts
                var simulationResult = _simulationEngine.SimulateProjectChange(project);

                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["WarningMessage"] = simulationResult.Message;
                    TempData["Conflicts"] = simulationResult.Conflicts;
                    return View(project);
                }

                _dataService.UpdateProject(project);
                TempData["SuccessMessage"] = $"Project '{project.Name}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id = project.ProjectID });
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public IActionResult Delete(int id)
        {
            var project = _dataService.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var project = _dataService.GetProjectById(id);
            if (project != null)
            {
                _dataService.DeleteProject(id);
                TempData["SuccessMessage"] = $"Project '{project.Name}' deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Simulate/5
        public IActionResult Simulate(int id)
        {
            var project = _dataService.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Tasks = _dataService.GetTasksByProjectId(id),
                AvailableCrews = _dataService.GetAllCrews(),
                Permits = _dataService.GetAllPermits(),
                ProjectConflicts = _conflictDetector.DetectAllConflicts(id),
                TotalProjectCost = _dataService.GetTasksByProjectId(id).Sum(t => t.Cost)
            };

            return View("Simulate", viewModel);
        }
    }
}