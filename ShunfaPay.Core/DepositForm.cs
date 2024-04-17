using Tgs.Payment.Clients;
using Tgs.Payment.ShunfaPay.Client;

namespace Tgs.Payment.ShunfaPay;
public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    ShunfaPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);