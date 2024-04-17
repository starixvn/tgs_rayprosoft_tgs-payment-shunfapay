namespace Tgs.Payment.ShunfaPay.Client;
public record ShunfaPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);