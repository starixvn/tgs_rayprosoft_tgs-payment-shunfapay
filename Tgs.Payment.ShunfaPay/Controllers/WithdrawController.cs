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
public class WithdrawController(ILogger<WithdrawController> logger) : ControllerBase
{
    private readonly ILogger<WithdrawController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Consumes("application/json")]
    [Produces(MediaTypeNames.Text.Plain)]
    [HttpPost("callback")]
    public async Task<string> WithdrawCallbackAsync(
        [FromServices] IGetService<GetWithdrawFormQuery, WithdrawForm?> getService,
        [FromServices] ShunfaPayHasher hasher,
        [FromServices] IPaymentServiceClient paymentServiceClient,
        [FromServices] IPaymentClient paymentClient,
        [FromBody] CallbackWithdrawViewModel source)    
    {
        _logger.LogInformation("{source}", source);

        var formId = Guid.Parse(source.BillNumber);

        var withdrawForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (withdrawForm == null)
        {
            _logger.LogWarning("Can't found withdraw form({id}).", formId);
            return "fail";
        }

        var apiKey = withdrawForm.ApplyInfo!.Settings.ApiKey;        

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(source))!;

        var sign = source.Sign;
        var hash = hasher.ComputeHash(
            apiKey,
            dict.Where(kv => kv.Value is not null)
                .Select(kv => new KeyValuePair<string, string?>(
                kv.Key,
                kv.Value!.ToString())));

        _logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            _logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var paymentReturn = new ShunfaPayWithdrawReturn()
        {
            FormId = formId,
            Amount = decimal.Parse(source.Amount),
            TotalAmount = decimal.Parse(source.TotalAmount),
            Sign = sign,
            Status = source.Status,
            Fee = decimal.Parse(source.Fee),
            Timestamp = source.Timestamp,
        };

        var fsm = PaymentStateMachineFactory.CreateWithdrawStateMachine(withdrawForm.Status);

        switch (paymentReturn.Status)
        {
            case "已完成":
                if (fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentWithdrawCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitWithdrawFormStatusAsync(
                        withdrawForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, ShunfaPayUtility.SerializerOptions)).ConfigureAwait(false);

                    await paymentClient
                        .PaidWithdrawAsync(
                            withdrawForm.Id,
                            withdrawForm.BrandId,
                            HttpContext.RequestAborted)
                        .ConfigureAwait(false);
                }

                break;

            case "失败":
                if (fsm.CanFire(PaymentTrigger.Reject))
                {
                    await fsm.FireAsync(PaymentTrigger.Reject).ConfigureAwait(false);

                    await paymentServiceClient.SubmitWithdrawFormStatusAsync(
                        withdrawForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, ShunfaPayUtility.SerializerOptions)).ConfigureAwait(false);

                    await paymentClient
                        .RejectWithdrawAsync(withdrawForm.Id, withdrawForm.BrandId)
                        .ConfigureAwait(false);
                }

                break;
        }

        return "OK";
    }
}