using Serilog;

namespace FamilyHubs.RequestForSupport.Web;

public static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.ConfigureHost();

            builder.Services.ConfigureServices(builder.Configuration);

            var app = builder.Build();

            ServiceProvider = app.ConfigureWebApplication();

            app.Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "An unhandled exception occurred during bootstrapping");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}