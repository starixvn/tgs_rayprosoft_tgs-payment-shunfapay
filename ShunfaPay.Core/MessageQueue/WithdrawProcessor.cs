using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.Payment.ShunfaPay.Client;
using Valhalla.MessageQueue;

namespace Tgs.Payment.ShunfaPay.MessageQueue;

internal class WithdrawProcessor(
    ShunfaPayClient shunfaPayClient,
    IOptions<ShunfaPayOptions> optionsAccessor,
    IPaymentClient paymentClient)
    : IMessageProcessor<WithdrawRequest, WithdrawResponse>
{
    private readonly ShunfaPayClient _shunfaPayClient = shunfaPayClient ?? throw new ArgumentNullException(nameof(shunfaPayClient));
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly ShunfaPayOptions _options = optionsAccessor.Value;

    public async ValueTask<WithdrawResponse> HandleAsync(
        string subject,
        WithdrawRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<ShunfaPayWithdrawSettings>(data.ApplyInfo, ShunfaPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(data.Amount),
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.Bank,
            settings.ReceiverName,
            settings.ReceiverAccount);

        var withdrawResponse = await _shunfaPayClient
            .WithdrawAsync(settings.ClientId, settings.ApiKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new ShunfaPayWithdrawApplyInfo(
            settings,
            new ShunfaPayWithdrawResponse(withdrawResponse.FormId, withdrawResponse.ThirdPartyOrderNo));

        await _paymentClient
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                settings.FormId,
                withdrawResponse.ThirdPartyOrderNo,
                cancellationToken)
            .ConfigureAwait(false);

        return new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, ShunfaPayUtility.SerializerOptions)
        };
    }
}