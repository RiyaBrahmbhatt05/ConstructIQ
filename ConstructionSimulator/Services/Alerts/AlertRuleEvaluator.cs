using ConstructionSimulator.Models;

namespace ConstructionSimulator.Services.Alerts
{
    public class AlertRuleEvaluator
    {
        public List<AlertNotification> Evaluate(
            List<Project> projects,
            List<ProjectTask> tasks,
            List<Permit> permits,
            decimal budgetThresholdPercent = 85m,
            int permitExpiryWindowDays = 14)
        {
            var alerts = new List<AlertNotification>();

            alerts.AddRange(BuildOverdueTaskAlerts(tasks));
            alerts.AddRange(BuildDependencyBlockedAlerts(tasks));
            alerts.AddRange(BuildBudgetThresholdAlerts(projects, budgetThresholdPercent));
            alerts.AddRange(BuildPermitExpiryAlerts(permits, permitExpiryWindowDays));

            return alerts
                .OrderByDescending(a => SeverityRank(a.Severity))
                .ThenByDescending(a => a.CreatedAtUtc)
                .ToList();
        }

        private static IEnumerable<AlertNotification> BuildOverdueTaskAlerts(List<ProjectTask> tasks)
        {
            var now = DateTime.UtcNow;

            foreach (var task in tasks.Where(t => t.IsDelayed))
            {
                yield return new AlertNotification
                {
                    Type = AlertType.TaskOverdue,
                    Severity = "High",
                    Title = "Task overdue",
                    Message = $"Task '{task.Name}' is overdue since {task.EndDate:MMM dd, yyyy}.",
                    EntityType = "Task",
                    EntityId = task.ProjectTaskId,
                    CreatedAtUtc = now
                };
            }
        }

        private static IEnumerable<AlertNotification> BuildDependencyBlockedAlerts(List<ProjectTask> tasks)
        {
            var now = DateTime.UtcNow;
            var tasksById = tasks.ToDictionary(t => t.ProjectTaskId, t => t);

            foreach (var task in tasks)
            {
                if (string.IsNullOrWhiteSpace(task.Dependencies))
                {
                    continue;
                }

                var dependencyIds = ParseDependencyIds(task.Dependencies);
                foreach (var depId in dependencyIds)
                {
                    if (!tasksById.TryGetValue(depId, out var dependencyTask))
                    {
                        continue;
                    }

                    if (!string.Equals(dependencyTask.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new AlertNotification
                        {
                            Type = AlertType.DependencyBlocked,
                            Severity = "Medium",
                            Title = "Dependency blocked",
                            Message = $"Task '{task.Name}' is blocked by dependency '{dependencyTask.Name}' ({dependencyTask.Status}).",
                            EntityType = "Task",
                            EntityId = task.ProjectTaskId,
                            CreatedAtUtc = now
                        };
                    }
                }
            }
        }

        private static IEnumerable<AlertNotification> BuildBudgetThresholdAlerts(List<Project> projects, decimal thresholdPercent)
        {
            var now = DateTime.UtcNow;

            foreach (var project in projects.Where(p => p.Budget > 0 && p.BudgetUtilization >= thresholdPercent))
            {
                var isCritical = project.BudgetUtilization >= 100m;

                yield return new AlertNotification
                {
                    Type = AlertType.BudgetThreshold,
                    Severity = isCritical ? "Critical" : "Medium",
                    Title = isCritical ? "Project over budget" : "Budget threshold reached",
                    Message = $"Project '{project.Name}' is at {project.BudgetUtilization:F1}% budget utilization.",
                    EntityType = "Project",
                    EntityId = project.ProjectId,
                    CreatedAtUtc = now
                };
            }
        }

        private static IEnumerable<AlertNotification> BuildPermitExpiryAlerts(List<Permit> permits, int windowDays)
        {
            var now = DateTime.UtcNow;
            var windowLimit = now.AddDays(windowDays);

            foreach (var permit in permits.Where(p => p.ExpiryDate.HasValue))
            {
                var expiry = permit.ExpiryDate!.Value;
                if (expiry < now)
                {
                    yield return new AlertNotification
                    {
                        Type = AlertType.PermitExpiry,
                        Severity = "Critical",
                        Title = "Permit expired",
                        Message = $"Permit '{permit.Type}' expired on {expiry:MMM dd, yyyy}.",
                        EntityType = "Permit",
                        EntityId = permit.PermitId,
                        CreatedAtUtc = now
                    };
                }
                else if (expiry <= windowLimit)
                {
                    yield return new AlertNotification
                    {
                        Type = AlertType.PermitExpiry,
                        Severity = "Medium",
                        Title = "Permit expiring soon",
                        Message = $"Permit '{permit.Type}' will expire on {expiry:MMM dd, yyyy}.",
                        EntityType = "Permit",
                        EntityId = permit.PermitId,
                        CreatedAtUtc = now
                    };
                }
            }
        }

        private static List<int> ParseDependencyIds(string rawDependencies)
        {
            return rawDependencies
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.TryParse(x, out var parsed) ? parsed : 0)
                .Where(x => x > 0)
                .Distinct()
                .ToList();
        }

        private static int SeverityRank(string severity)
        {
            return severity.ToLowerInvariant() switch
            {
                "critical" => 4,
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }
    }
}
