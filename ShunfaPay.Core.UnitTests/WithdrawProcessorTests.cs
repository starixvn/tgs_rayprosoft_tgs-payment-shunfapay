using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.Payment.ShunfaPay;
using Tgs.Payment.ShunfaPay.Client;
using Tgs.Payment.ShunfaPay.MessageQueue;

namespace ShunfaPay.Core.UnitTests;

public class WithdrawProcessorTests
{
    [Fact]
    public async Task 代付GCash()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://shunfapay.com");

        var fakePayClient = new ShunfaPayClient(
            httpClient,
            new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance),
            NullLogger<ShunfaPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new ShunfaPayOptions
        {
            CallbackBaseUrl = new Uri("https://dev-payment.rpgp.cc/shunfapay"),
            PaymentApiUrl = new Uri("https://shunfapay.com"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };

        var sut = new WithdrawProcessor(
            fakePayClient,
            new OptionsWrapper<ShunfaPayOptions>(options),
            fakePaymentClient);

        var formId = Guid.Parse("f664fa01-ccd7-4b4f-898e-08a712aa8564");
        var clientId = "20230323150810518";
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        var applySettings = new ShunfaPayWithdrawSettings(
            clientId,
            apiKey,
            formId,
            Bank: "gcash",
            ReceiverName: "091122334455",
            ReceiverAccount: "testPerson");

        var request = new WithdrawRequest
        {
            Amount = "100",
            ApplyInfo = JsonSerializer.Serialize(applySettings, ShunfaPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://shunfapay.com/api/v3/withdrawals")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "system_bill_number": "APO20240416112505212",
                    "amount": "100",
                    "fee": 0,
                    "status": "\u7b49\u5f85",
                    "total_amount": 100,
                    "client_id": "20230323150810518",
                    "bill_number": "f664fa01-ccd7-4b4f-898e-08a712aa8564",
                    "timestamp": "2024-04-16 11:25:35",
                    "sign": "de20565c9d035435ba67d14c1374faf8"
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.shunfapay.withdraw",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<ShunfaPayWithdrawApplyInfo>(actual.Result, ShunfaPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("APO20240416112505212"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}