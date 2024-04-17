using Tgs.Payment.Clients;
using Tgs.Payment.ShunfaPay.Client;

namespace Tgs.Payment.ShunfaPay;
public record WithdrawForm(
      Guid Id,
      string? UserId,
      UserType UserType,
      Guid BrandId,
      Guid? BrandPaymentId,
      decimal Amount,
      PaymentFlowState Status,
      Guid BankInfoId,
      string? BankInfo,
      ShunfaPayWithdrawApplyInfo? ApplyInfo,
      string? PaymentReturn = null);