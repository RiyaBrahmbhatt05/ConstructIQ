using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.Models
{
    public class Material
    {
        public int MaterialID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = "Units";   

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPerUnit { get; set; }

        [Range(0, int.MaxValue)]
        public int LeadTimeDays { get; set; } = 0;

        public string Supplier { get; set; } = string.Empty;

        public bool InStock { get; set; } = true;

        // Calculated properties
        public decimal TotalCost => Quantity * CostPerUnit;
        public DateTime? ExpectedDeliveryDate => LeadTimeDays > 0
            ? DateTime.Now.AddDays(LeadTimeDays)
            : null;
    }
}

