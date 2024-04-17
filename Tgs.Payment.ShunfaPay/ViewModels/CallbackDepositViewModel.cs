using System.Text.Json.Serialization;

namespace Tgs.Payment.ShunfaPay.ViewModels;

public class CallbackDepositViewModel
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = default!;

    [JsonPropertyName("bill_number")]
    public string BillNumber { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = default!;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = default!;

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = default!;
}