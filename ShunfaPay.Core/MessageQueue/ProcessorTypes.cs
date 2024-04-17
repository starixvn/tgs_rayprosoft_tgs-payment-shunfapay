namespace Tgs.Payment.ShunfaPay.MessageQueue;

public static class ProcessorTypes
{
    public static readonly Type DepositProcessorType = typeof(DepositProcessor);

    public static readonly Type WithdrawProcessorType = typeof(WithdrawProcessor);

    //public static readonly Type DepositTimeoutHandlerType = typeof(DepositTimeoutHandler);
    public static readonly Type GetBalanceProcessorType = typeof(GetBalanceProcessor);
}