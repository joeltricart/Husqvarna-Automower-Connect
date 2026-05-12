namespace HusqvarnaAutomowerConnect.App.ViewModels;

public sealed record MowerDashboardCard(
    string Id,
    string Name,
    string Details,
    string StatusLine,
    string BatteryLine,
    string ConnectivityLine,
    string LocationLine,
    string UpdatedLine,
    string ErrorLine);
