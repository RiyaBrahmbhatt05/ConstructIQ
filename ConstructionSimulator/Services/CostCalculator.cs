using ConstructionSimulator.Data;

namespace ConstructionSimulator.Services
{
    public class CostCalculator
    {
        private readonly InMemoryDataService _dataService;

        public CostCalculator(InMemoryDataService dataService)
        {
            _dataService = dataService;
        }

        public decimal CalculateTaskCost(Models.Task task)
        {
            decimal totalCost = 0;

            // Labor cost
            if (task.CrewID.HasValue)
            {
                var crew = _dataService.GetCrewById(task.CrewID.Value);
                if (crew != null)
                {
                  
                    var laborCost = crew.HourlyRate * 8 * task.Duration * crew.TeamSize;
                    totalCost += laborCost;
                }
            }

           
            totalCost += task.Cost;

            return totalCost;
        }

        public decimal CalculateProjectCost(int projectId)
        {
            var tasks = _dataService.GetTasksByProjectId(projectId);
            return tasks.Sum(t => t.Cost);
        }

        public decimal CalculateMaterialsCost()
        {
            var materials = _dataService.GetAllMaterials();
            return materials.Sum(m => m.TotalCost);
        }

        public decimal CalculatePermitCosts(int projectId)
        {
            var tasks = _dataService.GetTasksByProjectId(projectId);
            var permitIds = tasks.Where(t => t.PermitID.HasValue).Select(t => t.PermitID!.Value).Distinct();

            decimal totalPermitCost = 0;
            foreach (var permitId in permitIds)
            {
                var permit = _dataService.GetPermitById(permitId);
                if (permit != null)
                {
                    totalPermitCost += permit.Fee;
                }
            }

            return totalPermitCost;
        }

        public Dictionary<string, decimal> GetCostBreakdown(int projectId)
        {
            var breakdown = new Dictionary<string, decimal>
            {
                { "Labor", 0 },
                { "Materials", 0 },
                { "Permits", CalculatePermitCosts(projectId) },
                { "Other", 0 }
            };

            var tasks = _dataService.GetTasksByProjectId(projectId);

            foreach (var task in tasks)
            {
                if (task.CrewID.HasValue)
                {
                    var crew = _dataService.GetCrewById(task.CrewID.Value);
                    if (crew != null)
                    {
                        breakdown["Labor"] += crew.HourlyRate * 8 * task.Duration * crew.TeamSize;
                    }
                }
                else
                {
                    breakdown["Other"] += task.Cost;
                }
            }

            breakdown["Materials"] = CalculateMaterialsCost();

            return breakdown;
        }

        public decimal CalculateProjectedOverrun(int projectId)
        {
            var project = _dataService.GetProjectById(projectId);
            if (project == null) return 0;

            var actualCost = project.ActualCost;
            var budget = project.Budget;

            return actualCost > budget ? actualCost - budget : 0;
        }
    }
}