using Quotes;
using Alerts;
using Configuration;

namespace Monitoring;

static class QuoteMonitor
{
    public static async Task MonitorStockQuote
    (
        string stock,
        double sellPrice,
        double buyPrice,
        Config configuration,
        TimeSpan timeSpan,
        CancellationToken stoppingToken
    )
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                StockQuoteResponse stockQuote = await BrapiClient.GetStockPriceAsync(stock, stoppingToken);

                if (stockQuote.Success is not null)
                {
                    double currentPrice = stockQuote.Success.results[0].data.regularMarketPrice;
                    AlertKind alert = Alerts.AlertDecision.MakeDecision(sellPrice, buyPrice, currentPrice);

                    if (alert is not AlertKind.None)
                    {
                        await EmailAlertSender.SendEmailAsync(configuration, alert, stock);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Erro {stockQuote.Error!.code ?? ""} na API: {stockQuote.Error!.message}");
                    Console.ResetColor();
                }
            }
            catch (OperationCanceledException)
            {
                // user stopped
                break;
            }
            catch (Exception apiException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro de conexão/inesperado: {apiException.Message}");
                Console.ResetColor();
            }

            await Task.Delay(timeSpan, stoppingToken);
        }
    }
}