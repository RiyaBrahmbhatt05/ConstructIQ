namespace ConstructionSimulator.ViewModels.Alerts
{
    public class AlertsDashboardViewModel
    {
        public int TotalAlerts { get; set; }
        public int CriticalCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public List<AlertListItemViewModel> AlertItems { get; set; } = new List<AlertListItemViewModel>();

        public bool HasAlerts => AlertItems.Any();
    }
}
