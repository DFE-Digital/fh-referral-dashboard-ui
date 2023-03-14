using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RclTemp.FamilyHubsUi.Models;

namespace RclTemp.FamilyHubsUi.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddFamilyHubsUi(this IServiceCollection services, IConfiguration configuration, string feedbackUrlConfigurationKey = "FamilyHubsUi:FeedbackUrl")
    {
        //todo: if we have root model that is composed of the other models, could have a single injection into layout
        //todo: have standard config section
        //
        //todo: AddDfeUi() or similar helper. default FeedbackUrl config at that level, to allow manually providing the url to PhaseBanner
        // could possibly use IOptions or similar (IOptionsSnapshot would be handy for updates without downtime, but we probably don't want the overhead for something so central?)
        var configurationHelper = new ConfigurationHelper(configuration);
        services.AddSingleton<IConfigurationHelper>(configurationHelper);
        services.AddSingleton<IPhaseBanner>(new PhaseBanner(configurationHelper, feedbackUrlConfigurationKey));
    }
}