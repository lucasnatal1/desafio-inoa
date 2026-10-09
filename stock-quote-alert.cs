using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

class StockQuoteAlert
{
    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: stock-quote-alert.exe <stock> <sell-price> <buy-price>");
            return;
        }

        string stock = args[0];
        double sellPrice = double.Parse(args[1]);
        double buyPrice = double.Parse(args[2]);

        Console.WriteLine($"Monitoring {stock} with sell price {sellPrice} and buy price {buyPrice}");
    }
}