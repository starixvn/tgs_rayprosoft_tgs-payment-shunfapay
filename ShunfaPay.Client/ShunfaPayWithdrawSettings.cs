namespace Tgs.Payment.ShunfaPay.Client;
public record ShunfaPayWithdrawSettings(
    string ClientId,
    string ApiKey,
    Guid FormId,
    string Bank,
    string ReceiverName,
    string ReceiverAccount);