namespace Tgs.Payment.ShunfaPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string Type);