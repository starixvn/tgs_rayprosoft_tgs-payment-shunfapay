using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using Tgs.Payment.Clients;
using Tgs.Payment.ShunfaPay;
using Tgs.Payment.ShunfaPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddKeyedSingleton("Nats",
    (sp, _) => new NatsConnection(NatsOpts.Default with
    {
        Url = "nats://localhost:4222"
    }))
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var clientId = "20230323150810518";
var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

//代收Gcash
var orderNo = Guid.NewGuid();
var amount = 500.00m;

//var depositsettings = new ShunfaPayDepositSettings(
//    clientId,
//    apiKey,
//    orderNo,
//    "02");

//var depositreturn = await paymentClient.DepositAsync(
//    "tgs.payment.shunfapay.deposit",
//    amount,
//    JsonSerializer.Serialize(depositsettings));
//logger.LogInformation($"deposit return: {depositreturn}");

//代付GCash
//var withdrawSettings = new ShunfaPayWithdrawSettings(
//    clientId,
//    apiKey,
//    orderNo,
//    "gcash",
//    "testPerson",
//    "091122334455");

//var withdrawReturn = await paymentClient.WithdrawAsync(
//                "tgs.payment.shunfapay.withdraw",
//                100M,
//                JsonSerializer.Serialize(withdrawSettings));
//logger.LogInformation($"Withdraw Return: {withdrawReturn}");

//餘額查詢
//var httpClient = new HttpClient
//{
//    BaseAddress = new Uri("https://shunfapay.com")
//};
//var client = new ShunfaPayClient(httpClient, new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance), NullLogger<ShunfaPayClient>.Instance);
//var query = await client.QueryBalanceAsync(clientId,apiKey);

//logger.LogInformation("{query}", query);

//代收訂單查詢
var queryFormId = Guid.Parse("76cbde62-5937-4948-8e7e-a7d381f619e3");

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://shunfapay.com")
};
var client = new ShunfaPayClient(httpClient, new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance), NullLogger<ShunfaPayClient>.Instance);
var depositQuery = await client.QueryDepositFormAsync(clientId, apiKey, queryFormId);

logger.LogInformation("{depositQuery}", depositQuery);