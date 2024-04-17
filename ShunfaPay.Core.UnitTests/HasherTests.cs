using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.ShunfaPay;

namespace ShunfaPay.Core.UnitTests;

public class HasherTests
{
    [Fact]
    public void 餘額查詢()
    {
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        var dict = new Dictionary<string, string?>
        {
            ["client_id"] = "20230323150810518",
        };
        var expected = "53c96ca325f1040f44bb83145787a410";

        var sut = new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代收訂單查詢()
    {
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        var dict = new Dictionary<string, string?>
        {
            ["client_id"] = "20230323150810518",
            ["bill_number"] = "76cbde62-5937-4948-8e7e-a7d381f619e3",
            ["timestamp"] = "2024-04-15 08:57:09",
        };

        var expected = "6347092232b6156922a259a6279f828c";

        var sut = new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代付訂單查詢()
    {
        var apiKey = "ee15b04eb6e41622355a2b601323bc5f";

        var dict = new Dictionary<string, string?>
        {
            ["client_id"] = "20230323150810518",
            ["bill_number"] = "f664fa01-ccd7-4b4f-898e-08a712aa8564",
            ["timestamp"] = "2024-04-17 02:11:39",
        };

        var expected = "d3544523bd330083870e16024444558e";

        var sut = new ShunfaPayHasher(NullLogger<ShunfaPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }
}