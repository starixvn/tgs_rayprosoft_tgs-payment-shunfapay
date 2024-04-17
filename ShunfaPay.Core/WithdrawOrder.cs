namespace Tgs.Payment.ShunfaPay;
public record WithdrawOrder(
    Guid FormId,
    decimal Amount,
    Uri CallbackUrl,
    string Bank,
    string ReceiverName,
    string ReceiverAccount);