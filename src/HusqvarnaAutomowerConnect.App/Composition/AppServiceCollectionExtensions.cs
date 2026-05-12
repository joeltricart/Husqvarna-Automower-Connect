using HusqvarnaAutomowerConnect.App.ViewModels;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Services;
using HusqvarnaAutomowerConnect.App.Services;
using HusqvarnaAutomowerConnect.Infrastructure.Authentication;
using HusqvarnaAutomowerConnect.Infrastructure.Api;
using HusqvarnaAutomowerConnect.Infrastructure.Configuration;
using HusqvarnaAutomowerConnect.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace HusqvarnaAutomowerConnect.App.Composition;

public static class AppServiceCollectionExtensions
{
    public static IServiceCollection AddHusqvarnaAutomowerConnect(this IServiceCollection services)
    {
        services.AddSingleton<IAppSettingsStore, LocalAppSettingsStore>();
        services.AddSingleton<ISecureTokenStore, PasswordVaultSecureTokenStore>();
        services.AddSingleton<IApplicationSecretStore, PasswordVaultApplicationSecretStore>();
        services.AddSingleton<IOAuthCallbackListener, LoopbackOAuthCallbackListener>();
        services.AddSingleton<IBrowserLauncher, SystemBrowserLauncher>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<CommandAvailabilityService>();
        services.AddSingleton<IMowerService, MowerService>();
        services.AddHttpClient<IHusqvarnaAuthClient, HusqvarnaAuthClient>(client =>
        {
            client.BaseAddress = new Uri(OfficialApiContract.AuthenticationBaseUrl);
        });
        services.AddHttpClient<IHusqvarnaApiClient, HusqvarnaApiClient>(client =>
        {
            client.BaseAddress = new Uri(OfficialApiContract.AutomowerBaseUrl);
        });

        services.AddTransient<LoginViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<MowerDetailsViewModel>();

        return services;
    }
}
