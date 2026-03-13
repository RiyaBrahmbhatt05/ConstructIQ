using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class Material
    {
        public int MaterialId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal CostPerUnit { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        public bool InStock { get; set; } = true;

        [Range(0, 365)]
        public int LeadTimeDays { get; set; }

        [StringLength(50)]
        public string UnitType { get; set; } = string.Empty;

        public string? Description { get; set; }

        public List<TaskMaterial> TaskMaterials { get; set; } = new();
    }
}