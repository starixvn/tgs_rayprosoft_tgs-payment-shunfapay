using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.Protos;
using Tgs.Payment.ShunfaPay.Client;
using Valhalla.MessageQueue;

namespace Tgs.Payment.ShunfaPay.MessageQueue;

internal class DepositProcessor(
    ShunfaPayClient shunfaPayClient,
    IOptions<ShunfaPayOptions> optionsAccessor,
    ILogger<DepositProcessor> logger)
    : IMessageProcessor<DepositRequest, DepositResponse>
{
    private readonly ShunfaPayOptions _options = optionsAccessor.Value;

    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{DepositApplyInfo}", data.ApplyInfo);

        var settings = JsonSerializer.Deserialize<ShunfaPayDepositSettings>(
            data.ApplyInfo,
            ShunfaPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.Type);

        var depositResponse = await shunfaPayClient
            .DepositAsync(settings.ClientId, settings.ApiKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new ShunfaPayDepositApplyInfo(
            settings,
            new ShunfaPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, ShunfaPayUtility.SerializerOptions)
        };
    }
}