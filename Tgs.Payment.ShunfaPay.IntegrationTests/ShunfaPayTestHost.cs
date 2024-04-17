using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;

namespace Tgs.Payment.ShunfaPay.IntegrationTests;
internal static class ShunfaPayTestHost
{
    public static WebApplicationFactory<Program> Factory => new WebApplicationFactory<Program>()
        .WithWebHostBuilder(hostBuilder => hostBuilder
            .ConfigureLogging((_, logging) => logging.ClearProviders())
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.Sources.Clear();

                _ = configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ShunfaPay:PaymentApiUrl"] = "https://shunfapay.com",
                    ["ShunfaPay:CallbackBaseUrl"] = "https://dev-payment.rpgp.cc/shunfapay",
                    ["ShunfaPay:PaymentServiceBaseUrl"] = "http://localhost:5541"
                });
            })
            .ConfigureServices(services => services
                .AddFakeNatsMessageQueue()
                .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
                    sp => new ConfigureNamedOptions<OtlpExporterOptions>(
                        Options.DefaultName,
                        exporterOptions => exporterOptions.TimeoutMilliseconds = 0))));
}
