using ConstructionSimulator.ViewModels.Alerts;

namespace ConstructionSimulator.Services.Alerts
{
    public interface IAlertService
    {
        AlertsDashboardViewModel GetDashboardAlerts();
    }
}
