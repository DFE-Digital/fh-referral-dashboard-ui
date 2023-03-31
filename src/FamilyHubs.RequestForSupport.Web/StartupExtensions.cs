using FamilyHubs.SharedKernel.Razor.Cookies;
using Microsoft.ApplicationInsights.Extensibility;
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

        services.AddRazorPages();

        //todo: add health checks
        // handle API failures as Degraded, so that App Services doesn't remove or replace the instance (all instances!) due to an API being down
        //services.AddHealthChecks();

        // enable strict-transport-security header on localhost
#if hsts_localhost
        services.AddHsts(o => o.ExcludedHosts.Clear());
#endif

        services.AddCookiePage(configuration);
    }

    public static IServiceProvider ConfigureWebApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        //app.UseAppSecurityHeaders();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        // UseExceptionHandler doesn't accept a path beginning with ~, which is how you usually need to specify a path to a RCL contained view

        //var errorPagePath = Path.Combine(app.Environment.ContentRootPath, "Pages\\Error.cshtml");

        //        app.UseExceptionHandler(errorPagePath.Substring(2));

        app.UseExceptionHandler("/Error/Index");

        //PathString.FromUriComponent(new Uri())
        //app.UseExceptionHandler(new ExceptionHandlerOptions
        //{
        //    ExceptionHandlingPath = new PathString()
        //});
        //    errorApp =>
        //{
        //    errorApp.Run(async context =>
        //    {
        //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //        context.Response.ContentType = "text/html";

        //        // Render the custom error page directly to the response stream
        //        var errorPage = "~/Pages/Error.cshtml";
        //        var razorPage = app.Services.GetRequiredService<IRazorPageFactory>()
        //            .CreatePage(errorPage, new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()));

        //        await razorPage.ExecuteAsync(new ViewContext()
        //        {
        //            HttpContext = context,
        //            ViewData = razorPage.ViewData,
        //            Writer = new StreamWriter(context.Response.Body),
        //        });

        //        //// Get the absolute path to the error page
        //        //var errorPagePath = Path.Combine(app.Environment.ContentRootPath, "Pages\\Error.cshtml");

        //        //// Render the error page
        //        //var errorPageContent = await File.ReadAllTextAsync(errorPagePath);
        //        //await context.Response.WriteAsync(errorPageContent, Encoding.UTF8);
        //    });
        //});

        //app.UseExceptionHandler(errorPagePath);
        app.UseStatusCodePagesWithReExecute("~/Pages/Error/{0}");

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
