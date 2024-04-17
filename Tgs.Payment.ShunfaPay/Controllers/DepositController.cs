using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.Payment.ShunfaPay.ApplicationServices;
using Tgs.Payment.ShunfaPay.Client;
using Tgs.Payment.ShunfaPay.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ShunfaPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController(ILogger<DepositController> logger) : ControllerBase
{
    private readonly ILogger<DepositController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Consumes("application/json")]
    [Produces(MediaTypeNames.Text.Plain)]
    [HttpPost("callback")]
    public async Task<string> DepositCallbackAsync(
       [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
       [FromServices] IPaymentServiceClient paymentServiceClient,
       [FromServices] IPaymentClient paymentClient,
       [FromServices] ShunfaPayHasher hasher,
       [FromBody] CallbackDepositViewModel source)

    {
        _logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.BillNumber);

        var depositForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (depositForm == null)
        {
            _logger.LogWarning("Can't found deposit form({id}).", formId);
            return "fail";
        }

        var apiKey = depositForm.ApplyInfo!.Settings.ApiKey;

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(source))!;

        var sign = source.Sign;
        var hash = hasher.ComputeHash(
            apiKey,
            dict.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value.ToString())));

        _logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            _logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var paymentReturn = new ShunfaPayDepositReturn()
        {
            OrderAmount = decimal.Parse(source.Amount),
            FormId = formId,
            Status = source.Status,
            Sign = source.Sign,
            ClientId = source.ClientId,
            Timestamp = source.Timestamp,
        };

        var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

        if (paymentReturn.Status == "已完成")
        {
            if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
            {
                await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                await paymentServiceClient.SubmitDepositFormStatusAsync(
                    depositForm.Id,
                    paymentReturn: JsonSerializer.Serialize(paymentReturn, ShunfaPayUtility.SerializerOptions),
                    cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                await paymentClient.ApproveDepositAsync(
                    depositForm.Id,
                    depositForm.BrandId,
                    paymentReturn.OrderAmount,
                    HttpContext.RequestAborted).ConfigureAwait(false);
            }
        }

        return "OK";
    }
}