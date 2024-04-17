using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.ShunfaPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;

namespace Tgs.Payment.ShunfaPay.IntegrationTests;

public class WithdrawTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代付回調_訂單狀態失敗()
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

        var formId = Guid.Parse("f664fa01-ccd7-4b4f-898e-08a712aa8564");
        var brandId = Guid.NewGuid();
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";
        var clientId = "20230323150810518";

        var settings = new ShunfaPayWithdrawSettings(
            clientId,
            apiKey,
            formId,
            "gcash",
            "testPerson",
            "091122334455");
        var withdrawResponse = new ShunfaPayWithdrawResponse(formId, "APO20240416112505212");

        var applyInfo = new ShunfaPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, ShunfaPayUtility.SerializerOptions)
            });

        var request = """
            {
                "client_id": "20230323150810518",
                "bill_number": "f664fa01-ccd7-4b4f-898e-08a712aa8564",
                "status": "\u5931\u8d25",
                "amount": "100.00",
                "fee": "0.00",
                "total_amount": "100.00",
                "timestamp": "2024-04-16 11:41:16",
                "remark": "Test091122334455",
                "sign": "043457771054c34f123edff3c0a9b687"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new ShunfaPayWithdrawReturn()
        {
            FormId = formId,
            Amount = decimal.Parse("100.00"),
            Sign = "043457771054c34f123edff3c0a9b687",
            Status = "\u5931\u8d25",
            Fee = decimal.Parse("0.00"),
            TotalAmount = decimal.Parse("100.00"),
            Timestamp = "2024-04-16 11:41:16",
        };

        Assert.Equal("OK", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, ShunfaPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代付回調_訂單狀態已完成()
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

        var formId = Guid.Parse("8682b2ae-feea-4341-9e02-f36230b9dad6");
        var brandId = Guid.NewGuid();
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";
        var clientId = "20230323150810518";

        var settings = new ShunfaPayWithdrawSettings(
            clientId,
            apiKey,
            formId,
            "gcash",
            "testPerson",
            "091122334455");
        var withdrawResponse = new ShunfaPayWithdrawResponse(formId, "APO20240416112505212");

        var applyInfo = new ShunfaPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, ShunfaPayUtility.SerializerOptions)
            });

        var request = """
            {
                "client_id": "20230323150810518",
                "bill_number": "8682b2ae-feea-4341-9e02-f36230b9dad6",
                "status": "\u5df2\u5b8c\u6210",
                "amount": "100.00",
                "fee": "0.00",
                "total_amount": "100.00",
                "timestamp": "2024-04-16 12:03:06",
                "sign": "d1ae355a2671de02da5d08478cec0c90"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new ShunfaPayWithdrawReturn()
        {
            FormId = formId,
            Amount = decimal.Parse("100.00"),
            Sign = "d1ae355a2671de02da5d08478cec0c90",
            Status = "\u5df2\u5b8c\u6210",
            Fee = decimal.Parse("0.00"),
            TotalAmount = decimal.Parse("100.00"),
            Timestamp = "2024-04-16 12:03:06",
        };

        Assert.Equal("OK", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, ShunfaPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}