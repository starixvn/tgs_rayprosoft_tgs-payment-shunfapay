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

public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理_GCash()
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
        var formId = Guid.Parse("76cbde62-5937-4948-8e7e-a7d381f619e3");
        var clientId = "20230323150810518";
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        var sut = new DepositProcessor(
            fakePayClient,
            new OptionsWrapper<ShunfaPayOptions>(options),
            NullLogger<DepositProcessor>.Instance);

        var applySettings = new ShunfaPayDepositSettings(
            clientId,
            apiKey,
            formId,
            "02");

        var request = new DepositRequest
        {
            Amount = "500",
            ApplyInfo = JsonSerializer.Serialize(applySettings, ShunfaPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://shunfapay.com/api/v3/deposit")
            .With(req => req.Content?.Headers.ContentType?.MediaType == "application/json")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "message": "\u6210\u529f\u8ba2\u5355\u4ea7\u751f",
                    "client_id": "20230323150810518",
                    "bill_number": "API20240415154009665",
                    "amount": "500.00",
                    "status": "\u7b49\u5f85",
                    "url": "https:\/\/go.drmpro.net\/orders\/API20240415154009665",
                    "timestamp": "2024-04-15 15:40:56",
                    "sign": "16cf1c06780b04e99bc1d1495a477e31"
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.shunfapay.deposit",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<ShunfaPayDepositApplyInfo>(actual.Result, ShunfaPayUtility.SerializerOptions);

        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new ShunfaPayDepositResponse(
                500m,
                applySettings.FormId,
                new Uri("https://go.drmpro.net/orders/API20240415154009665"),
                "API20240415154009665"),
            applyInfo.Response);
    }
}