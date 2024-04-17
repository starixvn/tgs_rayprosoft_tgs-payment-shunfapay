namespace Tgs.Payment.ShunfaPay;

internal class ApiFailException : Exception
{
    public ApiFailException()
    {
    }

    public ApiFailException(string? message) : base(message)
    {
    }
}