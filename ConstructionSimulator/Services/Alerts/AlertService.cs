using ConstructionSimulator.Data;
using ConstructionSimulator.ViewModels.Alerts;

namespace ConstructionSimulator.Services.Alerts
{
    public class AlertService : IAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly AlertRuleEvaluator _ruleEvaluator;

        public AlertService(ApplicationDbContext context, AlertRuleEvaluator ruleEvaluator)
        {
            _context = context;
            _ruleEvaluator = ruleEvaluator;
        }

        public AlertsDashboardViewModel GetDashboardAlerts()
        {
            var projects = _context.Projects.ToList();
            var tasks = _context.Tasks.ToList();
            var permits = _context.Permits.ToList();

            var alerts = _ruleEvaluator.Evaluate(projects, tasks, permits);

            var mappedAlerts = alerts
                .Take(12)
                .Select(a => new AlertListItemViewModel
                {
                    Type = a.Type.ToString(),
                    Severity = a.Severity,
                    Title = a.Title,
                    Message = a.Message,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    CreatedAtUtc = a.CreatedAtUtc
                })
                .ToList();

            return new AlertsDashboardViewModel
            {
                TotalAlerts = alerts.Count,
                CriticalCount = alerts.Count(a => a.Severity == "Critical"),
                HighCount = alerts.Count(a => a.Severity == "High"),
                MediumCount = alerts.Count(a => a.Severity == "Medium"),
                AlertItems = mappedAlerts
            };
        }
    }
}
