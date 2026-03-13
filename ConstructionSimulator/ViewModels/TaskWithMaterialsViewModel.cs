using ConstructionSimulator.Models;
using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.ViewModels
{
    public class TaskWithMaterialsViewModel
    {
        public ProjectTask Task { get; set; } = new ProjectTask();

        public List<TaskMaterialInputViewModel> Materials { get; set; } = new();
    }
}