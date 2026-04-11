namespace ConstructionSimulator.ViewModels
{
    public class ProjectGanttViewModel
    {
        public int ProjectTaskId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public decimal ProgressPercent { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string BarColor { get; set; } = "#6b7280";
        public int StartOffsetDays { get; set; }
        public int BarWidthDays { get; set; }
        public string DependencyTaskIds { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }
}
