using System.Text.Json.Serialization;

namespace Tgs.Payment.ShunfaPay.ViewModels;

public class CallbackWithdrawViewModel
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = default!;

    [JsonPropertyName("bill_number")]
    public string BillNumber { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = default!;

    [JsonPropertyName("fee")]
    public string Fee { get; set; } = default!;

    [JsonPropertyName("total_amount")]
    public string TotalAmount { get; set; } = default!;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = default!;

    [JsonPropertyName("remark")]
    public string? Remark { get; set; }

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = default!;
}