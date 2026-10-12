using Configuration;
using Monitoring;
using Quotes;

namespace StockQuoteAlert;

static class StockQuoteAlert
{
    private static readonly TimeSpan TimeSpan = TimeSpan.FromSeconds(30);

    static async Task Main(string[] args)
    {
        try
        {
            await ValidateParams(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n{ex.Message}\n");
            Console.WriteLine("Aplicativo finalizado com erro.");
            return;
        }

        string stock = args[0].ToUpper();
        double sellPrice = double.Parse(args[1]);
        double buyPrice = double.Parse(args[2]);

        Console.WriteLine("========================================");
        Console.WriteLine("   STOCK QUOTE ALERT   ");
        Console.WriteLine("========================================\n");

        Console.WriteLine($"Monitoring {stock} with sell price {sellPrice} and buy price {buyPrice} every {TimeSpan.Seconds}s\n");

        string configPath = ConfigurationLoader.ReadConfigurationPath();
        Console.WriteLine($"Using configuration file: {configPath}\n");

        Config configurations = ConfigurationLoader.ReadConfiguration(configPath);

        using CancellationTokenSource cts = new();
        EnvReader.Load(".env");
        string? brapiApiKey = Environment.GetEnvironmentVariable("BRAPI_API_KEY");
        BrapiClient.UseApiKey(brapiApiKey);

        Task monitorStockQuote = QuoteMonitor.MonitorStockQuote(stock, sellPrice, buyPrice, configurations, TimeSpan, cts.Token);

        while (!cts.Token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nParada solicitada pelo usuário.");
                    cts.Cancel();
                }
            }

            await Task.Delay(100);
        }

        try
        {
            await monitorStockQuote;
        }
        catch (OperationCanceledException)
        {
            // token cancelled
        }

        Console.WriteLine("\nAplicativo finalizado com sucesso.");
    }

    static async Task ValidateParams(string[] args)
    {
        if (args.Length != 3)
        {
            throw new ArgumentException("Usage: stock-quote-alert.exe <stock> <sell-price> <buy-price>");
        }

        string stock = args[0].ToUpper();
        double sellPrice = double.Parse(args[1]);
        double buyPrice = double.Parse(args[2]);

        if (sellPrice <= buyPrice)
        {
            throw new ArgumentException("Usage: <sell-price> must be bigger than <buy-price>");
        }

        bool isStockValid = await BrapiClient.CheckStockSymbol(stock);
        if (!isStockValid)
        {
            throw new ArgumentException("Usage: <stock> must be an active stock symbol");
        }
    }
}
