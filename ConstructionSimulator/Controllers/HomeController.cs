using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.Data;
using ConstructionSimulator.Services;
using ConstructionSimulator.ViewModels;
using System.Diagnostics;

namespace ConstructionSimulator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ConflictDetector _conflictDetector;
        private readonly CostCalculator _costCalculator;

        public HomeController(ApplicationDbContext context, ConflictDetector conflictDetector, CostCalculator costCalculator)
        {
            _context = context;
            _conflictDetector = conflictDetector;
            _costCalculator = costCalculator;
        }

        public IActionResult Index()
        {
           
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                // User not logged in - redirect to login
                return RedirectToAction("Login", "Account");
            }

            // Store user info in ViewData for navbar
            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserEmail"] = userEmail;

            // Get all projects and tasks
            var projects = _context.Projects.ToList();
            var allTasks = _context.Tasks.ToList();

            //Build dashboard view model
            var viewModel = new DashboardViewModel
            {
                TotalProjects = projects.Count,
                ActiveProjects = projects.Count(p => p.Status == "In Progress"),
                CompletedProjects = projects.Count(p => p.Status == "Completed"),
                TotalBudget = projects.Sum(p => p.Budget),
                TotalActualCost = projects.Sum(p => p.ActualCost),
                TotalTasks = allTasks.Count,
                CompletedTasks = allTasks.Count(t => t.Status == "Completed"),
                OverdueTasks = allTasks.Count(t => t.IsDelayed),
                RecentProjects = projects.OrderByDescending(p => p.CreatedDate).Take(5).ToList(),
                UpcomingTasks = allTasks
                    .Where(t => t.Status == "Pending" && t.StartDate >= DateTime.Now)
                    .OrderBy(t => t.StartDate)
                    .Take(5)
                    .ToList()
            };

            // Detect conflicts in active projects
            foreach (var project in projects.Where(p => p.Status != "Completed"))
            {
                var projectConflicts = _conflictDetector.DetectAllConflicts(project.ProjectId);
                viewModel.Conflicts.AddRange(projectConflicts);
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}