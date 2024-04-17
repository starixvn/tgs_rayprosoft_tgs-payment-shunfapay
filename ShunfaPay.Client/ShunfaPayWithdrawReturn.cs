namespace Tgs.Payment.ShunfaPay.Client;

public class ShunfaPayWithdrawReturn
{
    /// <summary>
    /// 交易金額+手續費
    /// </summary>
    public decimal TotalAmount { get; set; }
    /// <summary>
    /// 交易金額
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// 手續費
    /// </summary>
    public decimal Fee { get; init; }
    /// <summary>
    /// 通知時間
    /// </summary>
    public string Timestamp { get; init; } = default!;

    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;
}