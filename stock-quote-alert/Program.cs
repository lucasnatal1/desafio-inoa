using Quotes;
namespace StockQuoteAlert;

static class Program
{
    static async Task Main(string[] args)
    {
        if (!TryParseArgs(args, out string stock, out double sellPrice, out double buyPrice, out string? error))
        {
            Console.WriteLine($"\n{error}\n");
            Console.WriteLine("Application stopped with an error.");
            return;
        }

        try
        {
            await TryValidateStockSymbol(stock);
        }
        catch (Exception argException)
        {
            Console.WriteLine($"\n{argException.Message}\n");
            Console.WriteLine("Application stopped with an error.");
            return;
        }

        using CancellationTokenSource cts = new();

        Task appTask = StockQuoteAlertApp.RunAsync(stock, sellPrice, buyPrice, cts.Token);

        while (!cts.Token.IsCancellationRequested && !appTask.IsCompleted)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nStop requested by the user.");
                    cts.Cancel();
                }
                else if (keyInfo.Key == ConsoleKey.H)
                {
                    StockQuoteAlertApp.PrintStatus();
                }
            }

            await Task.Delay(100);
        }

        try
        {
            await appTask;
        }
        catch (OperationCanceledException)
        {
            // token cancelled
        }
        catch (Exception)
        {
            return;
        }

        Console.WriteLine("\nApplication stopped successfully.");
    }

    static bool TryParseArgs(
        string[] args,
        out string stock,
        out double sellPrice,
        out double buyPrice,
        out string? error)
    {
        stock = string.Empty;
        sellPrice = 0;
        buyPrice = 0;
        error = null;

        if (args.Length != 3)
        {
            error = "Usage: stock-quote-alert.exe <stock> <sell-price> <buy-price>";
            return false;
        }

        stock = args[0].ToUpperInvariant();

        if (!double.TryParse(args[1], out sellPrice) || !double.TryParse(args[2], out buyPrice))
        {
            error = "Usage: <sell-price> and <buy-price> must be valid numbers";
            return false;
        }

        if (sellPrice <= buyPrice)
        {
            error = "Usage: <sell-price> must be bigger than <buy-price>";
            return false;
        }

        return true;
    }

    static async Task TryValidateStockSymbol(string stock)
    {
        try
        {
            bool isStockValid = await BrapiClient.IsActiveStockSymbol(stock);
            if (!isStockValid)
                throw new ArgumentException("Usage: <stock> must be an active stock symbol");
        }
        catch
        {
            throw new ArgumentException("Usage: <stock> must be an active stock symbol");
        }
    }
}
