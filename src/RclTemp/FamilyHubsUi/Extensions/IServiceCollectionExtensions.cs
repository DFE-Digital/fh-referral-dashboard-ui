using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RclTemp.FamilyHubsUi.Options;

namespace RclTemp.FamilyHubsUi.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddFamilyHubsUi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FamilyHubsUiOptions>(configuration.GetSection(FamilyHubsUiOptions.FamilyHubsUi));

        //var configurationHelper = new ConfigurationHelper(configuration);
        //services.AddSingleton<IConfigurationHelper>(configurationHelper);
    }
}