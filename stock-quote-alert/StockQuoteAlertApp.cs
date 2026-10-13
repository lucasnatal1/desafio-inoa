using Configuration;
using Monitoring;
using Quotes;

namespace StockQuoteAlert;

static class StockQuoteAlertApp
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private static MonitoringStatus? _status;

    public static void PrintStatus()
    {
        if (_status is null)
        {
            Console.WriteLine("\nMonitoring has not started yet. Press H again after the first quote.\n");
            return;
        }

        _status.PrintHelp();
    }

    public static async Task RunAsync(
        string stock,
        double sellPrice,
        double buyPrice,
        CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine("========================================");
            Console.WriteLine("   STOCK QUOTE ALERT   ");
            Console.WriteLine("========================================\n");

            Console.WriteLine(
                $"Monitoring {stock} with sell price {sellPrice} and buy price {buyPrice} every {PollInterval.Seconds}s");

            string configPath = ConfigurationLoader.ReadConfigurationPath();
            Console.WriteLine($"Using configuration file: {configPath}\n");

            Config configurations = ConfigurationLoader.ReadConfiguration(configPath);

            EnvReader.Load(".env");
            string? brapiApiKey = Environment.GetEnvironmentVariable("BRAPI_API_KEY");
            BrapiClient.UseApiKey(brapiApiKey);

            _status = new MonitoringStatus(stock, sellPrice, buyPrice);

            await QuoteMonitor.MonitorStockQuote(
                stock,
                sellPrice,
                buyPrice,
                configurations,
                PollInterval,
                _status,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // stop requested by the host
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n{ex.Message}\n");
            Console.WriteLine("Application stopped with an error.");
            throw;
        }
    }
}
