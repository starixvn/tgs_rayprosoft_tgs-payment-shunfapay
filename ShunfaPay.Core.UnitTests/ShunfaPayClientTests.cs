using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.ShunfaPay;

namespace ShunfaPay.Core.UnitTests;

public class ShunfaPayClientTests
{
    [Fact]
    public async Task 餘額查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://shunfapay.com");
        var signer = new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance);

        var sut = new ShunfaPayClient(httpClient, signer, NullLogger<ShunfaPayClient>.Instance);

        var clientId = "20230323150810518";
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        _ = mockHttp.Expect(HttpMethod.Post, "https://shunfapay.com/api/v3/balance")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "client_id": "20230323150810518",
                    "available_amount": "388.70",
                    "freeze_amount": "0.00",
                    "waiting_deposit": "0.00",
                    "arriving_deposit": "0.00",
                    "unsettled_amount": "121.77",
                    "timestamp": "2024-04-16 11:57:26",
                    "sign": "6a22f9a8e7f45a1f2d0f270ade1ba7a3"
                }
                """));

        var actual = await sut.QueryBalanceAsync(clientId, apiKey);

        Assert.Equal(388.70m, actual);
    }

    [Fact]
    public async Task 代收GCash()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://shunfapay.com");
        var signer = new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance);

        var sut = new ShunfaPayClient(httpClient, signer, NullLogger<ShunfaPayClient>.Instance);

        var formId = Guid.Parse("76cbde62-5937-4948-8e7e-a7d381f619e3");
        var clientId = "20230323150810518";
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        _ = mockHttp.Expect(HttpMethod.Post, "https://shunfapay.com/api/v3/deposit")
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

        var order = new DepositOrder(
           500.00m,
           formId,
           new Uri("https://dev-payment.rpgp.cc/shunfapay/deposit/callback"),
           "02");

        var actual = await sut.DepositAsync(clientId, apiKey, order);

        Assert.Equal("API20240415154009665", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://go.drmpro.net/orders/API20240415154009665"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代收訂單查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://shunfapay.com");
        var signer = new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance);

        var sut = new ShunfaPayClient(httpClient, signer, NullLogger<ShunfaPayClient>.Instance);

        var formId = Guid.Parse("76cbde62-5937-4948-8e7e-a7d381f619e3");
        var clientId = "20230323150810518";
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        _ = mockHttp.Expect(HttpMethod.Post, "https://shunfapay.com/api/v3/deposit/query")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "code": 0,
                    "message": "查询成功，请确认定单状态",
                    "client_id": "20230323150810518",
                    "bill_number": "76cbde62-5937-4948-8e7e-a7d381f619e3",
                    "amount": "500.00",
                    "status": "已完成",
                    "timestamp": "2024-04-15 16:55:13",
                    "sign": "4632025bbf0207f1ca1198e61cf166b6"
                }
                """));

        var actual = await sut.QueryDepositFormAsync(clientId, apiKey, formId);

        Assert.Equal("已完成", actual);
    }

    [Fact]
    public async Task 代付GCash()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://shunfapay.com");
        var signer = new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance);

        var sut = new ShunfaPayClient(httpClient, signer, NullLogger<ShunfaPayClient>.Instance);

        var formId = Guid.Parse("f664fa01-ccd7-4b4f-898e-08a712aa8564");
        var clientId = "20230323150810518";
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        _ = mockHttp.Expect(HttpMethod.Post, "https://shunfapay.com/api/v3/withdrawals")
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

        var order = new WithdrawOrder(
           formId,
           500.00m,
           new Uri("https://dev-payment.rpgp.cc/shunfapay/withdraw/callback"),
           "gcash",
           "testPerson",
           "091122334455");

        var actual = await sut.WithdrawAsync(clientId, apiKey, order);

        Assert.Equal("APO20240416112505212", actual.ThirdPartyOrderNo);
    }
}