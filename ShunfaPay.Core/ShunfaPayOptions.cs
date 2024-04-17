namespace Tgs.Payment.ShunfaPay;
public class ShunfaPayOptions
{

    public required Uri CallbackBaseUrl { get; set; }

    public required Uri PaymentApiUrl { get; set; }

    public required Uri PaymentServiceBaseUrl { get; set; }
}
