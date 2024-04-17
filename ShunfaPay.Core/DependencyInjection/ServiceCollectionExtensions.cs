using System.Reflection;
using Tgs.Payment.ShunfaPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShunfaPay(
        this IServiceCollection services,
        Action<ShunfaPayOptions, IServiceProvider> configureOptions,
        Action<IHttpClientBuilder> ConfigureAiClient)
    {
        ConfigureAiClient?.Invoke(services.AddHttpClient<ShunfaPayClient>());

        return services
            .AddOptions<ShunfaPayOptions>()
            .Configure(configureOptions)
            .Services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddSingleton<ShunfaPayHasher>();
    }
}