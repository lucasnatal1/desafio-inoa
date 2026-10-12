using System.Net.Http.Headers;
using System.Text.Json;

namespace Quotes;

static class BrapiClient
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<StockQuoteResponse> GetStockPriceAsync(string stock, CancellationToken stoppingToken)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Fetching quote for {stock}...");

        string url = $"https://brapi.dev/api/v2/stocks/quote?symbols={stock}";
        HttpResponseMessage response = await _httpClient.GetAsync(url, stoppingToken);
        string responseBody = await response.Content.ReadAsStringAsync(stoppingToken);

        if (response.IsSuccessStatusCode)
        {
            StockQuoteSuccessResponse stockQuoteResponse = JsonSerializer.Deserialize<StockQuoteSuccessResponse>(responseBody)!;
            return new StockQuoteResponse() { Success = stockQuoteResponse };
        }
        else
        {
            StockQuoteErrorResponse stockQuoteResponse = JsonSerializer.Deserialize<StockQuoteErrorResponse>(responseBody)!;
            return new StockQuoteResponse() { Error = stockQuoteResponse };

        }
    }

    public static async Task<bool> CheckStockSymbol(string symbol)
    {
        string url = $"https://brapi.dev/api/v2/tickers?search={symbol}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"BRAPI returned {(int)response.StatusCode} while checking the stock symbol.");

        TickerListResponse tickerResponse = JsonSerializer.Deserialize<TickerListResponse>(responseBody)
            ?? throw new JsonException("BRAPI returned an empty ticker list.");

        foreach (TickerListItem result in tickerResponse.results)
        {
            if (result.isActive && result.symbol == symbol)
                return true;
        }

        return false;
    }

    public static void UseApiKey(string? brapiApiKey)
    {
        if (string.IsNullOrEmpty(brapiApiKey))
            return;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", brapiApiKey);
    }
}
