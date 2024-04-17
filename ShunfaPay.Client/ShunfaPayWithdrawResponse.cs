namespace Tgs.Payment.ShunfaPay.Client;
public record ShunfaPayWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo);