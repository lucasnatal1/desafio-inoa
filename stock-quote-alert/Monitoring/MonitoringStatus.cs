using Alerts;

namespace Monitoring;

sealed class MonitoringStatus
{
    public MonitoringStatus(string stock, double sellPrice, double buyPrice)
    {
        Stock = stock;
        SellPrice = sellPrice;
        BuyPrice = buyPrice;
    }

    public string Stock { get; }
    public double SellPrice { get; }
    public double BuyPrice { get; }
    public double? LatestQuote { get; private set; }
    public AlertKind LatestAlert { get; private set; } = AlertKind.None;

    public void Update(double currentPrice, AlertKind alert)
    {
        LatestQuote = currentPrice;
        LatestAlert = alert;
    }

    public void PrintHelp()
    {
        Console.WriteLine("\n--- HELP ---");
        Console.WriteLine($"Stock: {Stock}");
        Console.WriteLine($"Sell limit: R${SellPrice:0.00}");
        Console.WriteLine($"Buy limit: R${BuyPrice:0.00}");

        if (LatestQuote.HasValue)
            Console.WriteLine($"Latest quote: R${LatestQuote.Value:0.00}");
        else
            Console.WriteLine("Latest quote: not available yet");

        Console.WriteLine($"Current recommendation: {FormatRecommendation(LatestAlert)}");
        Console.WriteLine("Press Esc to stop.");
        Console.WriteLine("------------\n");
    }

    static string FormatRecommendation(AlertKind alert)
    {
        switch (alert)
        {
            case AlertKind.Sell:
                return "Sell";
            case AlertKind.Buy:
                return "Buy";
            default:
                return "None";

        }
    }
}
