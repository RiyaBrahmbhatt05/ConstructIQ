using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class TaskMaterial
    {
        public int TaskMaterialId { get; set; }

        [Required]
        public int ProjectTaskId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitsRequired { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SubtotalCost { get; set; }

        // Navigation properties
        public ProjectTask? ProjectTask { get; set; }
        public Material? Material { get; set; }
    }
}