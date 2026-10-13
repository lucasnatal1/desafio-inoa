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
        MonitoringStatus status,
        CancellationToken stoppingToken
    )
    {
        Console.WriteLine("Press H for status. Press Esc to stop.\n");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                StockQuoteResponse stockQuote = await BrapiClient.GetStockPriceAsync(stock, stoppingToken);

                if (stockQuote.Success is not null)
                {
                    double currentPrice = stockQuote.Success.results[0].data.regularMarketPrice;
                    AlertKind alert = AlertDecision.MakeDecision(sellPrice, buyPrice, currentPrice);

                    if (alert is not AlertKind.None && status.LatestAlert != alert)
                    {
                        await EmailAlertSender.SendEmailAsync(configuration, alert, stock, currentPrice);
                    }

                    status.Update(currentPrice, alert);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"API error {stockQuote.Error!.code ?? ""}: {stockQuote.Error!.message}");
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
                Console.WriteLine($"Unexpected connection error: {apiException.Message}");
                Console.ResetColor();
            }

            await Task.Delay(timeSpan, stoppingToken);
        }
    }
}