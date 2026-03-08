using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.Data;
using ConstructionSimulator.Services;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Controllers
{
    public class HomeController : Controller
    {
        private readonly InMemoryDataService _dataService;
        private readonly ConflictDetector _conflictDetector;
        private readonly CostCalculator _costCalculator;

        public HomeController(InMemoryDataService dataService, ConflictDetector conflictDetector, CostCalculator costCalculator)
        {
            _dataService = dataService;
            _conflictDetector = conflictDetector;
            _costCalculator = costCalculator;
        }

        public IActionResult Index()
        {
            var projects = _dataService.GetAllProjects();
            var allTasks = _dataService.GetAllTasks();

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

            // Collect all conflicts from all projects
            foreach (var project in projects.Where(p => p.Status != "Completed"))
            {
                var projectConflicts = _conflictDetector.DetectAllConflicts(project.ProjectID);
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
            return View();
        }
    }
}