using ConstructionSimulator.Data;
using Microsoft.EntityFrameworkCore;

namespace ConstructionSimulator.Services
{
    public class CostCalculator
    {
        private readonly ApplicationDbContext _context;

        public CostCalculator(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal CalculateTaskCost(Models.ProjectTask task)
        {
            decimal materialCost = CalculateTaskMaterialCost(task.ProjectTaskId);
            decimal crewCost = CalculateTaskCrewCost(task);

            return materialCost + crewCost;
        }

        public decimal CalculateTaskMaterialCost(int projectTaskId)
        {
            var taskMaterials = _context.TaskMaterials
                .Include(tm => tm.Material)
                .Where(tm => tm.ProjectTaskId == projectTaskId)
                .ToList();

            decimal total = 0;

            foreach (var taskMaterial in taskMaterials)
            {
                if (taskMaterial.Material != null)
                {
                    var subtotal = taskMaterial.Material.CostPerUnit * taskMaterial.UnitsRequired;
                    taskMaterial.SubtotalCost = subtotal;
                    total += subtotal;
                }
            }

            return total;
        }

        public decimal CalculateTaskCrewCost(Models.ProjectTask task)
        {
            if (!task.CrewId.HasValue)
                return 0;

            var crew = _context.Crews.FirstOrDefault(c => c.CrewId == task.CrewId.Value);
            if (crew == null)
                return 0;

            decimal crewCost = crew.HourlyRate * 8 * task.Duration;
            return crewCost;
        }

        public decimal RecalculateAndSaveTaskCost(int projectTaskId)
        {
            var task = _context.Tasks.FirstOrDefault(t => t.ProjectTaskId == projectTaskId);
            if (task == null)
                return 0;

            decimal materialCost = CalculateTaskMaterialCost(projectTaskId);
            decimal crewCost = CalculateTaskCrewCost(task);

            task.Cost = materialCost + crewCost;

            _context.SaveChanges();

            return task.Cost;
        }

        public decimal CalculateProjectCost(int projectId)
        {
            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            return tasks.Sum(t => t.Cost);
        }

        public decimal CalculateMaterialsCost()
        {
            var materials = _context.Materials.ToList();
            return materials.Sum(m => m.CostPerUnit * m.Quantity);
        }

        public decimal CalculatePermitCosts(int projectId)
        {
            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            var permitIds = tasks
                .Where(t => t.PermitId.HasValue)
                .Select(t => t.PermitId!.Value)
                .Distinct();

            decimal totalPermitCost = 0;

            foreach (var permitId in permitIds)
            {
                var permit = _context.Permits.FirstOrDefault(p => p.PermitId == permitId);
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

            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            foreach (var task in tasks)
            {
                if (task.CrewId.HasValue)
                {
                    var crew = _context.Crews.FirstOrDefault(c => c.CrewId == task.CrewId.Value);
                    if (crew != null)
                    {
                        breakdown["Labor"] += crew.HourlyRate * 8 * task.Duration;
                    }
                }

                breakdown["Materials"] += CalculateTaskMaterialCost(task.ProjectTaskId);
            }

            return breakdown;
        }

        public decimal CalculateProjectedOverrun(int projectId)
        {
            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null) return 0;

            var expectedCost = CalculateProjectCost(projectId);
            return expectedCost > project.Budget ? expectedCost - project.Budget : 0;
        }
    }
}