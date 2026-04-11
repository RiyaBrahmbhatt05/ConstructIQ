namespace ConstructionSimulator.ViewModels.Alerts
{
    public class AlertListItemViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
