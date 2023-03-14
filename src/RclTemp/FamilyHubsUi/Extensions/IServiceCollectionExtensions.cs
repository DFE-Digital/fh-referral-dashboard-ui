using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RclTemp.FamilyHubsUi.Models;
using RclTemp.FamilyHubsUi.Options;

namespace RclTemp.FamilyHubsUi.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddFamilyHubsUi(this IServiceCollection services, IConfiguration configuration)
    {
        //var familyHubsUiOptions = configuration.GetSection(FamilyHubsUiOptions.FamilyHubsUi)
        //    .Get<FamilyHubsUiOptions>();

        //if (familyHubsUiOptions == null)
        //{
        //    //todo: nice error message
        //    throw new NotImplementedException();
        //}

        //todo: tryparse with nice (fail fast) error message
        //var phase = Enum.Parse<Phase>(familyHubsUiOptions.Phase);

        services.Configure<FamilyHubsUiOptions>(configuration.GetSection(FamilyHubsUiOptions.FamilyHubsUi));

        //todo: add whole of options to service collection IOptions, after verifying everything

        //todo: if we have root model that is composed of the other models, could have a single injection into layout
        // could possibly use IOptions or similar (IOptionsSnapshot would be handy for updates without downtime, but we probably don't want the overhead for something so central?)
        var configurationHelper = new ConfigurationHelper(configuration);
        services.AddSingleton<IConfigurationHelper>(configurationHelper);
        //services.AddSingleton<IPhaseBanner>(new PhaseBanner(phase, familyHubsUiOptions.FeedbackUrl));
    }
}