namespace ConstructionSimulator.Models
{
    public class AlertNotification
    {
        public AlertType Type { get; set; }
        public string Severity { get; set; } = "Info";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsAcknowledged { get; set; }
    }
}
