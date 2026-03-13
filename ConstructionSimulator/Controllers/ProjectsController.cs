using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using ConstructionSimulator.Services;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SimulationEngine _simulationEngine;
        private readonly ConflictDetector _conflictDetector;
        private readonly CostCalculator _costCalculator;

        public ProjectsController(
     ApplicationDbContext context,
     SimulationEngine simulationEngine,
     ConflictDetector conflictDetector,
     CostCalculator costCalculator)
        {
            _context = context;
            _simulationEngine = simulationEngine;
            _conflictDetector = conflictDetector;
            _costCalculator = costCalculator;
        }

        // GET: Projects
        public IActionResult Index()
        {
            var projects = _context.Projects.ToList();
            return View(projects);
        }

        // GET: Projects/Details/5
        public IActionResult Details(int id)
        {

            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            var tasks = _context.Tasks
                .Where(t => t.ProjectId == id)
                .ToList();

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Tasks = tasks,
                AvailableCrews = _context.Crews.ToList(),
                Permits = _context.Permits.ToList(),
                ProjectConflicts = _conflictDetector.DetectAllConflicts(id)
            };

            viewModel.CompletedTasksCount = viewModel.Tasks.Count(t => t.Status == "Completed");
            viewModel.PendingTasksCount = viewModel.Tasks.Count(t => t.Status == "Pending");
            viewModel.TotalProjectCost = viewModel.Tasks.Sum(t => t.Cost);

            var costBreakdown = _costCalculator.GetCostBreakdown(project.ProjectId);
            ViewBag.CostBreakdown = costBreakdown;

            ViewBag.ExpectedCost = viewModel.TotalProjectCost;
            ViewBag.BudgetDifference = project.Budget - viewModel.TotalProjectCost;
            ViewBag.IsOverBudget = viewModel.TotalProjectCost > project.Budget;
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

                _context.Projects.Add(project);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Project '{project.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        public IActionResult Edit(int id)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
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
            if (id != project.ProjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProject = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
                if (existingProject == null)
                {
                    return NotFound();
                }

                // Keep existing simulation logic for now
                var simulationResult = _simulationEngine.SimulateProjectChange(project);

                if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
                {
                    TempData["WarningMessage"] = simulationResult.Message;
                    return View(project);
                }

                existingProject.Name = project.Name;
                existingProject.Description = project.Description;
                existingProject.StartDate = project.StartDate;
                existingProject.EndDate = project.EndDate;
                existingProject.Budget = project.Budget;
                existingProject.Status = project.Status;
               // existingProject.Priority = project.Priority;
               // existingProject.ProgressPercentage = project.ProgressPercentage;
                existingProject.ActualCost = project.ActualCost;

                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Project '{project.Name}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id = project.ProjectId });
            }

            return View(project);
        }

        // GET: Projects/Delete/5
        public IActionResult Delete(int id)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
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
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
            if (project != null)
            {
                var relatedTasks = _context.Tasks.Where(t => t.ProjectId == id).ToList();
                var relatedLogs = _context.SimulationLogs.Where(l => l.ProjectId == id).ToList();

                if (relatedTasks.Any())
                {
                    _context.Tasks.RemoveRange(relatedTasks);
                }

                if (relatedLogs.Any())
                {
                    _context.SimulationLogs.RemoveRange(relatedLogs);
                }

                _context.Projects.Remove(project);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Project '{project.Name}' deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Simulate/5
        public IActionResult Simulate(int id)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            var tasks = _context.Tasks
                .Where(t => t.ProjectId == id)
                .ToList();

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Tasks = tasks,
                AvailableCrews = _context.Crews.ToList(),
                Permits = _context.Permits.ToList(),
                ProjectConflicts = _conflictDetector.DetectAllConflicts(id),
                TotalProjectCost = tasks.Sum(t => t.Cost)
            };

            return View("Simulate", viewModel);
        }
    }
}