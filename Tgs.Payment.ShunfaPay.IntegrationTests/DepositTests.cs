using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.ShunfaPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.ShunfaPay.IntegrationTests;

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代收回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = ShunfaPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("54e853ab-5393-4ac0-84fd-6c335839f22b");
        var brandId = Guid.NewGuid();
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        var request = """
            {
                "client_id": "20230323150810518",
                "amount": "500.00",
                "bill_number": "54e853ab-5393-4ac0-84fd-6c335839f22b",
                "status": "\u5df2\u5b8c\u6210",
                "timestamp": "2024-04-16 10:33:40",
                "sign": "4e8be12de9f0dc90b073ce989c87016c"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new ShunfaPayDepositApplyInfo(
            new ShunfaPayDepositSettings(
                "20230323150810518",
                apiKey,
                formId,
                "02"),
             new ShunfaPayDepositResponse(
                500m,
                formId,
                new Uri("https://go.drmpro.net/orders/API20240416103005695"),
                "API20240416103005695"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 500m,
                ThirdPartyOrderNo = "API20240416103005695",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, ShunfaPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("OK", actual);

        var paymentReturn = new ShunfaPayDepositReturn()
        {
            Status = "已完成",
            FormId = formId,
            OrderAmount = decimal.Parse("500.00"),
            Sign = "4e8be12de9f0dc90b073ce989c87016c",
            ClientId = "20230323150810518",
            Timestamp = "2024-04-16 10:33:40",
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, ShunfaPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.OrderAmount),
                Arg.Any<CancellationToken>());
    }
}