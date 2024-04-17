namespace Tgs.Payment.ShunfaPay;
public record ShunfaPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);