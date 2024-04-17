namespace Tgs.Payment.ShunfaPay.Client;
public record ShunfaPayDepositSettings(
    string ClientId,
    string ApiKey,
    Guid FormId,
    string Type);