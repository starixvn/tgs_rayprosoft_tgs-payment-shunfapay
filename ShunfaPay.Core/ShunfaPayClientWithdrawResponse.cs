namespace Tgs.Payment.ShunfaPay;
public record ShunfaPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);