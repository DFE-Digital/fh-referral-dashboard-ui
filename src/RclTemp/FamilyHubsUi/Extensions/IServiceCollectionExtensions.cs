using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RclTemp.FamilyHubsUi.Options;

namespace RclTemp.FamilyHubsUi.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddFamilyHubsUi(this IServiceCollection services, IConfiguration configuration)
    {
        //var familyHubsUi = configuration.GetSection(FamilyHubsUiOptions.FamilyHubsUi).Get<FamilyHubsUiOptions>();
        ////todo: can pass object instead?
        //services.Configure<FamilyHubsUiOptions>(o => o = familyHubsUi);

        //var configurationHelper = new ConfigurationHelper(configuration);
        //services.AddSingleton<IConfigurationHelper>(configurationHelper);

        services.Configure<FamilyHubsUiOptions>(configuration.GetSection(FamilyHubsUiOptions.FamilyHubsUi))
            .AddSingleton<IValidateOptions<FamilyHubsUiOptions>, FamilyHubsUiOptionsValidation>()
            .AddSingleton<IConfigureOptions<FamilyHubsUiOptions>, FamilyHubsUiOptionsConfigure>();
        //todo: should replace the above
            //.ConfigureOptions<FamilyHubsUiOptions>();

        //dataannotations only??
        //services.ValidateOnStart();
        //.ValidateOnStart();


        return services;
    }
}