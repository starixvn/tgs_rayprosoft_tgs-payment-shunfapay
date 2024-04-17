using System.Text.Json;
using Tgs.Payment.Protos;
using Tgs.Payment.ShunfaPay.Client;
using Valhalla.MessageQueue;

namespace Tgs.Payment.ShunfaPay.MessageQueue;

internal class GetBalanceProcessor(
    ShunfaPayClient shunfaPayClient)
    : IMessageProcessor<PaymentBalanceRequest, PaymentBalanceResponse>
{
    public async ValueTask<PaymentBalanceResponse> HandleAsync(
        string subject,
        PaymentBalanceRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<BrandPaymentMerchantInfo>(
            data.BrandPaymentSettings,
            ShunfaPayUtility.SerializerOptions)!;

        var result = await shunfaPayClient.QueryBalanceAsync(
            settings.ClientId,
            settings.ApiKey,
            cancellationToken).ConfigureAwait(false);

        return new PaymentBalanceResponse
        {
            Balance = result
        };
    }
}