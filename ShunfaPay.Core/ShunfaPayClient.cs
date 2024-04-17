using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.ShunfaPay;

public class ShunfaPayClient(
    HttpClient httpClient,
    ShunfaPayHasher hasher,
    ILogger<ShunfaPayClient> logger)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ShunfaPayHasher _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
    private readonly ILogger<ShunfaPayClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask<ShunfaPayClientDepositResponse> DepositAsync(
        string clientId,
        string apiKey,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["bill_number"] = order.FormId.ToString(),
            ["type"] = order.Type,
            ["amount"] = order.Amount.ToString(),
            ["notify_url"] = order.CallbackUrl.AbsoluteUri,
        };

        var sign = _hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        var response = await _httpClient
            .PostAsJsonAsync("/api/v3/deposit", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("ShunfaPayClient Deposit response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(node["message"]!.ToString());

        return new ShunfaPayClientDepositResponse(
            Amount: decimal.Parse(node["amount"]!.GetValue<string>()),
            FormId: order.FormId,
            ThirdPartyOrderNo: node["bill_number"]!.GetValue<string>(),
            RedirectUrl: new Uri(node["url"]!.GetValue<string>()));
    }

    public async ValueTask<string> QueryDepositFormAsync(
        string clientId,
        string apiKey,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["bill_number"] = formId.ToString(),
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
        };

        var sign = _hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        var response = await _httpClient
            .PostAsJsonAsync($"/api/v3/deposit/query", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("ShunfaPayClient QueryDepositForm response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(node["message"]!.ToString());

        return node["status"]!.GetValue<string>();
    }

    public async ValueTask<ShunfaPayClientWithdrawResponse> WithdrawAsync(
        string clientId,
        string apiKey,
        WithdrawOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["bill_number"] = order.FormId.ToString(),
            ["amount"] = order.Amount.ToString("#.##"),
            ["notify_url"] = order.CallbackUrl.AbsoluteUri,
            ["receiver_name"] = order.ReceiverName,
            ["receiver_account"] = order.ReceiverAccount,
            ["bank"] = order.Bank,
            ["bank_branch"] = "N/A",
        };

        if (order.Bank == "gcash")
        {
            dict["remark"] = order.ReceiverAccount;
        }
        else
            dict["remark"] = "no";

        var sign = _hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        _logger.LogInformation("ShunfaPay Withdraw request Send sign:{sign}", sign);

        var response = await _httpClient
            .PostAsJsonAsync("/api/v3/withdrawals", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("ShunfaPayClient Withdraw response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Withdraw response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(node["message"]!.ToString());

        return new ShunfaPayClientWithdrawResponse(
            FormId: Guid.Parse(node["bill_number"]!.GetValue<string>()),
            ThirdPartyOrderNo: node["system_bill_number"]!.GetValue<string>(),
            Amount: decimal.Parse(node["amount"]!.GetValue<string>()));
    }

    public async ValueTask<decimal> QueryBalanceAsync(
        string clientId,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
        };

        var sign = _hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        var response = await _httpClient
            .PostAsJsonAsync("/api/v3/balance", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("ShunfaPayClient Query Balance response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Balance response can't read.");

        if (node["code"]!.GetValue<int>() != 0)
            throw new ApiFailException(node["message"]!.ToString());

        return decimal.Parse(node["available_amount"]!.GetValue<string>());
    }
}