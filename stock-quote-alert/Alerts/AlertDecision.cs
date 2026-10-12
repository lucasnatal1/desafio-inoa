namespace Alerts;

static class AlertDecision
{
    public static AlertKind MakeDecision(double sellPrice, double buyPrice, double currentPrice)
    {
        Console.WriteLine(currentPrice);
        if (currentPrice > sellPrice)
        {
            return AlertKind.Sell;
        }

        if (currentPrice < buyPrice)
        {
            return AlertKind.Buy;
        }

        return AlertKind.None;
    }
}