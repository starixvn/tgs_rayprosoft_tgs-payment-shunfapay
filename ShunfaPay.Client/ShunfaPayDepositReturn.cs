namespace Tgs.Payment.ShunfaPay.Client;
public record ShunfaPayDepositReturn
{
    public decimal OrderAmount { get; init; }
    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;

    public string ClientId { get; set; } = default!;

    public string Timestamp { get; set; } = default!;
}