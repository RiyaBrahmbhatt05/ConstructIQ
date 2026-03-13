using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.ViewModels
{
    public class TaskMaterialInputViewModel
    {
        [Required]
        public int MaterialId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitsRequired { get; set; }
    }
}