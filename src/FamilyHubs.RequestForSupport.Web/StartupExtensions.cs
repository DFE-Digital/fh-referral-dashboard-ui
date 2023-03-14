using Microsoft.ApplicationInsights.Extensibility;
using RclTemp.Models;
using Serilog;
using Serilog.Events;

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

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddSingleton<ITelemetryInitializer, TelemetryPiiRedactor>();
        services.AddApplicationInsightsTelemetry();

        // Add services to the container.
        services.AddRazorPages();

        //todo: add health checks
        // handle API failures as Degraded, so that App Services doesn't remove or replace the instance (all instances!) due to an API being down
        //services.AddHealthChecks();

        // enable strict-transport-security header on localhost
#if hsts_localhost
        services.AddHsts(o => o.ExcludedHosts.Clear());
#endif


        //todo: AddDfeUi() or similar helper. default FeedbackUrl config at that level, to allow manually providing the url to PhaseBanner
        // could possibly use IOptions or similar (IOptionsSnapshot would be handy for updates without downtime, but we probably don't want the overhead for something so central?)
        var configurationHelper = new ConfigurationHelper(configuration);
        services.AddSingleton<IConfigurationHelper>(configurationHelper);
        services.AddSingleton<IPhaseBanner>(new PhaseBanner(configurationHelper, "FeedbackUrl"));
    }

    public static IServiceProvider ConfigureWebApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        //app.UseAppSecurityHeaders();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        //todo: add error pages
        //app.UseStatusCodePagesWithReExecute("/Error/{0}");

#if use_https
        app.UseHttpsRedirection();
#endif
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        //todo: add health checks
        //app.MapHealthChecks("/health", new HealthCheckOptions
        //{
        //    Predicate = _ => true,
        //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        //});

        return app.Services;
    }
}
