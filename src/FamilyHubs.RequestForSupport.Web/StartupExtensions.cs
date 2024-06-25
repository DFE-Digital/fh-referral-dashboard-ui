using FamilyHubs.Notification.Api.Client.Extensions;
using FamilyHubs.Notification.Api.Client.Templates;
using FamilyHubs.RequestForSupport.Core.ApiClients;
using FamilyHubs.RequestForSupport.Web.Pages.Vcs;
using FamilyHubs.SharedKernel.GovLogin.AppStart;
using FamilyHubs.SharedKernel.Identity;
using FamilyHubs.SharedKernel.DataProtection;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;
using System.Diagnostics.CodeAnalysis;
using FamilyHubs.RequestForSupport.Infrastructure.Health;
using FamilyHubs.SharedKernel.Razor.Health;

namespace FamilyHubs.RequestForSupport.Web;

public static class StartupExtensions
{
    public static void ConfigureHost(this WebApplicationBuilder builder)
    {
        // ApplicationInsights
        builder.Host.UseSerilog((_, services, loggerConfiguration) =>
        {
            var logLevelString = builder.Configuration["LogLevel"];

            var parsed = Enum.TryParse<LogEventLevel>(logLevelString, out var logLevel);

            loggerConfiguration.WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces,
                parsed ? logLevel : LogEventLevel.Warning);

            loggerConfiguration.WriteTo.Console(
                parsed ? logLevel : LogEventLevel.Warning);
        });
    }

    public static void ConfigureServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        const string dataProtectionAppName = "Connect";
        services.AddFamilyHubsDataProtection(configuration, dataProtectionAppName);

        //services.AddSingleton<ITelemetryInitializer, TelemetryPiiRedactor>();
        services.AddApplicationInsightsTelemetry();

        services.AddAndConfigureGovUkAuthentication(configuration);

        // Add services to the container.
        services.AddHttpClients(configuration);

        services.AddNotificationsApiClient(configuration);
        services.AddSingleton<INotificationTemplates<NotificationType>, NotificationTemplates<NotificationType>>();

        services.AddRazorPages();

        //todo: add health checks
        // handle API failures as Degraded, so that App Services doesn't remove or replace the instance (all instances!) due to an API being down
        //services.AddHealthChecks();

        // enable strict-transport-security header on localhost
#if hsts_localhost
        services.AddHsts(o => o.ExcludedHosts.Clear());
#endif

        services.AddSiteHealthChecks(configuration);

        services.AddFamilyHubs(configuration);
    }

    public static void AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSecuredTypedHttpClient<IReferralClientService, ReferralClientService>((serviceProvider, httpClient) =>
        {
            httpClient.BaseAddress = new Uri(configuration.GetValue<string>("ReferralApiUrl")!);
        });
    }

    public static IServiceCollection AddSecuredTypedHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
            where TClient : class
            where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>((serviceProvider, httpClient) =>
        {
            configureClient(serviceProvider, httpClient);
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            if (httpContextAccessor == null)
                throw new ArgumentException($"IHttpContextAccessor required for {nameof(AddSecuredTypedHttpClient)}");

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {httpContextAccessor.HttpContext!.GetBearerToken()}");

        });

        return services;
    }

    public static IServiceProvider ConfigureWebApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseFamilyHubsDataProtection();

        app.UseFamilyHubs();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

#if use_https
        app.UseHttpsRedirection();
#endif
        app.UseStaticFiles();

        app.UseRouting();

        app.UseGovLoginAuthentication();

        app.MapRazorPages();

        app.MapFamilyHubsHealthChecks(typeof(StartupExtensions).Assembly);

        return app.Services;
    }
}
