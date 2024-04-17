namespace Tgs.Payment.ShunfaPay.Client;
public record ShunfaPayWithdrawApplyInfo(
    ShunfaPayWithdrawSettings Settings,
    ShunfaPayWithdrawResponse Response);