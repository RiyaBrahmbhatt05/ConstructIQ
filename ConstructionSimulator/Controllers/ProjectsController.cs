using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using ConstructionSimulator.Services;
using ConstructionSimulator.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ConflictDetector _conflictDetector;
        private readonly CostCalculator _costCalculator;
        private readonly SimulationEngine _simulationEngine;

        public ProjectsController(
            ApplicationDbContext context,
            ConflictDetector conflictDetector,
            CostCalculator costCalculator,
            SimulationEngine simulationEngine)
        {
            _context = context;
            _conflictDetector = conflictDetector;
            _costCalculator = costCalculator;
            _simulationEngine = simulationEngine;
        }

        // GET: Projects
        public IActionResult Index()
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var projects = _context.Projects
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            return View(projects);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            return View(new Project
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                Status = "Planning"
            });
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Project project)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            if (project.EndDate < project.StartDate)
            {
                ModelState.AddModelError(nameof(project.EndDate), "End date must be on or after start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            project.CreatedDate = DateTime.Now;
            _context.Projects.Add(project);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Project '{project.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Details/5
        public IActionResult Details(int id)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            var tasks = _context.Tasks
                .Where(t => t.ProjectId == id)
                .OrderBy(t => t.StartDate)
                .ToList();

            var ganttStart = tasks.Any() ? tasks.Min(t => t.StartDate).Date : project.StartDate.Date;
            var ganttEnd = tasks.Any() ? tasks.Max(t => t.EndDate).Date : project.EndDate.Date;
            var ganttSpanDays = Math.Max((ganttEnd - ganttStart).Days, 1);

            var ganttTasks = tasks.Select(task =>
            {
                var startOffsetDays = Math.Max((task.StartDate.Date - ganttStart).Days, 0);
                var barWidthDays = Math.Max((task.EndDate.Date - task.StartDate.Date).Days + 1, 1);
                var isOverdue = task.EndDate.Date < DateTime.Today && task.Status != "Completed";
                var progressPercent = task.Status == "Completed"
                    ? 100m
                    : task.Status == "In Progress"
                        ? 60m
                        : task.Status == "Blocked"
                            ? 15m
                            : 0m;

                var barColor = isOverdue
                    ? "#dc2626"
                    : task.Status == "Completed"
                    ? "#10b981"
                    : task.Status == "In Progress"
                        ? "#3b82f6"
                        : task.Status == "Blocked"
                            ? "#ef4444"
                            : task.Priority == "Critical"
                                ? "#f59e0b"
                                : "#6b7280";

                return new ProjectGanttViewModel
                {
                    ProjectTaskId = task.ProjectTaskId,
                    Name = task.Name,
                    Description = task.Description,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    DurationDays = Math.Max(task.Duration, 1),
                    ProgressPercent = progressPercent,
                    Status = task.Status,
                    Priority = task.Priority,
                    BarColor = barColor,
                    StartOffsetDays = startOffsetDays,
                    BarWidthDays = barWidthDays,
                    DependencyTaskIds = task.Dependencies ?? string.Empty,
                    IsOverdue = isOverdue
                };
            }).ToList();

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Tasks = tasks,
                GanttTasks = ganttTasks,
                AvailableCrews = _context.Crews.ToList(),
                Permits = _context.Permits.ToList(),
                TotalProjectCost = _costCalculator.CalculateProjectCost(id),
                CompletedTasksCount = tasks.Count(t => t.Status == "Completed"),
                PendingTasksCount = tasks.Count(t => t.Status != "Completed"),
                ProjectConflicts = _conflictDetector.DetectAllConflicts(id)
            };

            var expectedCost = _costCalculator.CalculateProjectCost(id);
            var budgetDifference = project.Budget - expectedCost;

            ViewBag.ExpectedCost = expectedCost;
            ViewBag.BudgetDifference = budgetDifference;
            ViewBag.IsOverBudget = expectedCost > project.Budget;
            ViewBag.CostBreakdown = _costCalculator.GetCostBreakdown(id);
            ViewBag.GanttStart = ganttStart;
            ViewBag.GanttEnd = ganttEnd;
            ViewBag.GanttSpanDays = ganttSpanDays;

            return View(viewModel);
        }

        // GET: Projects/Edit/5
        public IActionResult Edit(int id)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

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
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            if (id != project.ProjectId)
            {
                return NotFound();
            }

            if (project.EndDate < project.StartDate)
            {
                ModelState.AddModelError(nameof(project.EndDate), "End date must be on or after start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            var existingProject = _context.Projects.FirstOrDefault(p => p.ProjectId == id);
            if (existingProject == null)
            {
                return NotFound();
            }

            var simulationResult = _simulationEngine.SimulateProjectChange(project);
            if (simulationResult.Conflicts.Any(c => c.Severity == "Critical"))
            {
                ModelState.AddModelError(string.Empty, simulationResult.Message);
                return View(project);
            }

            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.Budget = project.Budget;
            existingProject.Status = project.Status;

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Project '{existingProject.Name}' updated successfully.";
            if (simulationResult.Conflicts.Any())
            {
                TempData["InfoMessage"] = simulationResult.Message;
            }

            return RedirectToAction(nameof(Details), new { id = existingProject.ProjectId });
        }

        // GET: Projects/Delete/5
        public IActionResult Delete(int id)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

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
        public IActionResult DeleteConfirmed(int projectId)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var projectTaskIds = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => t.ProjectTaskId)
                .ToList();

            if (projectTaskIds.Any())
            {
                var taskMaterials = _context.TaskMaterials
                    .Where(tm => projectTaskIds.Contains(tm.ProjectTaskId))
                    .ToList();
                if (taskMaterials.Any())
                {
                    _context.TaskMaterials.RemoveRange(taskMaterials);
                }
            }

            var logs = _context.SimulationLogs
                .Where(l => l.ProjectId == projectId)
                .ToList();
            if (logs.Any())
            {
                _context.SimulationLogs.RemoveRange(logs);
            }

            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();
            if (tasks.Any())
            {
                _context.Tasks.RemoveRange(tasks);
            }

            _context.Projects.Remove(project);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Project '{project.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult? RedirectIfNotLoggedIn()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            return null;
        }
    }
}
