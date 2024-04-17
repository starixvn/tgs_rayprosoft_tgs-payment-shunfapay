using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.Protos;
using Tgs.Payment.ShunfaPay;
using Tgs.Payment.ShunfaPay.MessageQueue;

namespace ShunfaPay.Core.UnitTests;

public class GetBalanceProcessorTests
{
    [Fact]
    public async Task GetBalanceProcessor_取得商戶餘額()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://shunfapay.com");

        var fakeClient = new ShunfaPayClient(
            httpClient,
            new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance),
            NullLogger<ShunfaPayClient>.Instance);

        var sut = new GetBalanceProcessor(fakeClient);

        var request = new PaymentBalanceRequest
        {
            BrandPaymentSettings = """
            {
                "clientId": "20230323150810518",
                "apiKey": "ee15b04eb6e41622355a2b601323bc5f"
            }
            """
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://shunfapay.com/api/v3/balance")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "client_id": "20230323150810518",
                    "available_amount": "44637.90",
                    "freeze_amount": "0.00",
                    "waiting_deposit": "0.00",
                    "arriving_deposit": "0.00",
                    "unsettled_amount": "0.00",
                    "timestamp": "2024-04-15 14:54:42",
                    "sign": "d3aafb18e611cf4a2b3b5beb956b873f"
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "tgs.payment.shunfapay.balance",
            request,
            null);

        // Assert
        Assert.Equal(44637.90M, actual.Balance.ToDecimal());
    }
}