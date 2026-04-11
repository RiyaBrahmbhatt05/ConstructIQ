using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.Data;
using ConstructionSimulator.Services;
using ConstructionSimulator.Services.Alerts;
using ConstructionSimulator.ViewModels;
using System.Diagnostics;

namespace ConstructionSimulator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ConflictDetector _conflictDetector;
        private readonly CostCalculator _costCalculator;
        private readonly IAlertService _alertService;

        public HomeController(
            ApplicationDbContext context,
            ConflictDetector conflictDetector,
            CostCalculator costCalculator,
            IAlertService alertService)
        {
            _context = context;
            _conflictDetector = conflictDetector;
            _costCalculator = costCalculator;
            _alertService = alertService;
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
            var crews = _context.Crews.ToList();

            // Calculate project health indicators
            var onTrackProjects = new List<Models.Project>();
            var atRiskProjects = new List<Models.Project>();
            var delayedProjects = new List<Models.Project>();

            foreach (var project in projects.Where(p => p.Status != "Completed"))
            {
                var projectTasks = allTasks.Where(t => t.ProjectId == project.ProjectId).ToList();
                var projectTaskCompletionRate = projectTasks.Any() ? (decimal)projectTasks.Count(t => t.Status == "Completed") / projectTasks.Count * 100 : 0;
                var projectConflicts = _conflictDetector.DetectAllConflicts(project.ProjectId);
                
                if (projectConflicts.Any(c => c.Severity == "Critical" || c.Severity == "High") || project.IsOverBudget)
                {
                    delayedProjects.Add(project);
                }
                else if (projectTaskCompletionRate < 50 || project.BudgetUtilization > 80)
                {
                    atRiskProjects.Add(project);
                }
                else
                {
                    onTrackProjects.Add(project);
                }
            }

            // Calculate crew utilization (hours worked per crew)
            var crewUtilization = crews.Select(c => new Models.Crew
            {
                CrewId = c.CrewId,
                Name = c.Name,
                SkillType = c.SkillType,
                TeamSize = c.TeamSize,
                HourlyRate = c.HourlyRate,
                IsAvailable = c.IsAvailable
            }).ToList();

            // Build chart data for budget vs actual
            var budgetChartProjects = projects.OrderByDescending(p => p.CreatedDate).Take(5).ToList();
            var budgetChartLabels = budgetChartProjects.Select(p => p.Name).ToList();
            var budgetChartBudgets = budgetChartProjects.Select(p => p.Budget).ToList();
            var budgetChartActuals = budgetChartProjects.Select(p => p.ActualCost).ToList();

            // Build task completion data
            var last4Weeks = Enumerable.Range(0, 4)
                .Select(i => DateTime.Now.AddDays(-i * 7))
                .OrderBy(d => d)
                .ToList();

            var taskCompletionLabels = last4Weeks.Select(d => $"Week {d:M/d}").ToList();
            var taskCompletionData = last4Weeks.Select(startDate => 
                allTasks.Count(t => t.Status == "Completed" && t.EndDate >= startDate && t.EndDate < startDate.AddDays(7))
            ).ToList();

            // Calculate metrics
            var pendingTasks = allTasks.Count(t => t.Status == "Pending");
            var completedTasks = allTasks.Count(t => t.Status == "Completed");
            var taskCompletionRate = allTasks.Any() ? (completedTasks * 100m) / allTasks.Count : 0;
            var totalBudget = projects.Sum(p => p.Budget);
            var totalActualCost = projects.Sum(p => p.ActualCost);
            var budgetVariance = totalBudget - totalActualCost;
            var budgetVariancePercent = totalBudget > 0 ? (budgetVariance / totalBudget) * 100 : 0;
            var averageCostPerTask = allTasks.Any() ? totalActualCost / allTasks.Count : 0;

            //Build dashboard view model
            var viewModel = new DashboardViewModel
            {
                TotalProjects = projects.Count,
                ActiveProjects = projects.Count(p => p.Status == "In Progress"),
                CompletedProjects = projects.Count(p => p.Status == "Completed"),
                OnTrackProjects = onTrackProjects.Count,
                AtRiskProjects = atRiskProjects.Count,
                DelayedProjects = delayedProjects.Count,
                
                TotalBudget = totalBudget,
                TotalActualCost = totalActualCost,
                BudgetVariance = budgetVariance,
                BudgetVariancePercent = budgetVariancePercent,
                
                TotalTasks = allTasks.Count,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                OverdueTasks = allTasks.Count(t => t.IsDelayed),
                TaskCompletionRate = taskCompletionRate,
                
                AverageCostPerTask = averageCostPerTask,
                TotalCrewHours = crews.Sum(c => c.TeamSize) * 8,
                AverageCrewUtilization = 75, // Placeholder - calculate from actual hours if data available
                
                BudgetChartLabels = budgetChartLabels,
                BudgetChartBudgets = budgetChartBudgets,
                BudgetChartActuals = budgetChartActuals,
                
                TaskCompletionChartLabels = taskCompletionLabels,
                TaskCompletionChartData = taskCompletionData,
                
                ProjectHealthOnTrackCount = onTrackProjects.Count,
                ProjectHealthAtRiskCount = atRiskProjects.Count,
                ProjectHealthDelayedCount = delayedProjects.Count,
                
                RecentProjects = projects.OrderByDescending(p => p.CreatedDate).Take(5).ToList(),
                UpcomingTasks = allTasks
                    .Where(t => t.Status == "Pending" && t.StartDate >= DateTime.Now)
                    .OrderBy(t => t.StartDate)
                    .Take(5)
                    .ToList(),
                CrewUtilization = crewUtilization,
                Alerts = _alertService.GetDashboardAlerts()
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