#:property PublishAot=false
#:property JsonSerializerIsReflectionEnabledByDefault=true

using System;
using System.Text.Json;
using System.Net;
using System.Net.Mail;

class StockQuoteAlert
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private static readonly TimeSpan TimeSpan = TimeSpan.FromSeconds(30);

    static async Task Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: stock-quote-alert.exe <stock> <sell-price> <buy-price>");
            return;
        }

        string stock = args[0];
        double sellPrice = double.Parse(args[1]);
        double buyPrice = double.Parse(args[2]);

        Console.WriteLine("========================================");
        Console.WriteLine("   STOCK QUOTE ALERT   ");
        Console.WriteLine("========================================\n");

        Console.WriteLine($"Monitoring {stock} with sell price {sellPrice} and buy price {buyPrice} every {TimeSpan.Seconds}s\n");

        string configPath = ReadConfigurationPath();
        Console.WriteLine($"Using configuration file: {configPath}\n");

        Config configurations = ReadConfiguration(configPath);

/*         Console.WriteLine($"recipientEmail: {string.Join(", ", configurations.recipientEmail)}");
        Console.WriteLine($"smtp host: {configurations.smtp.host}");
        Console.WriteLine($"smtp port: {configurations.smtp.port}");
        Console.WriteLine($"smtp ssl: {configurations.smtp.enableSsl}");
        Console.WriteLine($"smtp username: {configurations.smtp.username}"); */

        using CancellationTokenSource cts = new CancellationTokenSource();

        Task watchStockValue = AsyncWatchStockValue(stock, sellPrice, buyPrice, cts.Token, configurations);

        while (!cts.Token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\n[Parada solicitada pelo usuário.]");
                    cts.Cancel();
                }
                await Task.Delay(100);
            }
        }

        try
        {
            await watchStockValue;
        }
        catch (OperationCanceledException)
        {
            // token cancelled
        }

        Console.WriteLine("\nAplicativo finalizado com sucesso.");
    }

    private static async Task AsyncWatchStockValue(string stock, double sellPrice, double buyPrice, CancellationToken stoppingToken, Config configurations)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Consultando API para o stock {stock}...");

                EnvReader.Load(".env");
                string? brapiApiKey = Environment.GetEnvironmentVariable("BRAPI_API_KEY");
                if (!string.IsNullOrEmpty(brapiApiKey))
                {
                    HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {brapiApiKey}");
                }

                HttpResponseMessage response = await HttpClient.GetAsync($"https://brapi.dev/api/v2/stocks/quote?symbols={stock}");
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    StockQuoteSuccessResponse stockQuoteResponse = JsonSerializer.Deserialize<StockQuoteSuccessResponse>(responseBody)!;
                    await CheckValues(sellPrice, buyPrice, stockQuoteResponse, configurations, stock);
                }
                else
                {
                    StockQuoteErrorResponse stockQuoteResponse = JsonSerializer.Deserialize<StockQuoteErrorResponse>(responseBody)!;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Erro {stockQuoteResponse.code ?? ""} na API: {stockQuoteResponse.message}");
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

            await Task.Delay(TimeSpan, stoppingToken);
        }
    }

    private static async Task CheckValues(double sellPrice, double buyPrice, StockQuoteSuccessResponse stockInfo, Config configurations, string stock)
    {
        double currentPrice = stockInfo.results[0].data.regularMarketPrice;
        Console.WriteLine(currentPrice);
        if (currentPrice > sellPrice)
        {
            await SendEmailAsync(configurations, "sell", stock);
            return;
        }

        if (currentPrice < buyPrice)
        {
            await SendEmailAsync(configurations, "buy", stock);
        }
    }

    static string ReadConfigurationPath()
    {
        while (true)
        {
            Console.WriteLine("Enter the path to the JSON configuration file. Type 'help' to see expected file.\n");
            string? configPath = Console.ReadLine()?.Trim();

            if (configPath == "help")
            {
                Console.WriteLine("\n =========== FILE EXAMPLE ===========");
                Console.WriteLine("{");
                Console.WriteLine("   recipientEmail: string[],");
                Console.WriteLine("   smtp: {");
                Console.WriteLine("      host: string");
                Console.WriteLine("      port: int?");
                Console.WriteLine("      enableSsl: bool?");
                Console.WriteLine("      username: string");
                Console.WriteLine("      password: string");
                Console.WriteLine("   }");
                Console.WriteLine("}");
                Console.WriteLine("=======================================\n");
                continue;
            }

            if (string.IsNullOrEmpty(configPath))
            {
                Console.WriteLine("Enter a file path.");
                continue;
            }

            if (!configPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\nConfiguration file must be a JSON file.");
                continue;
            }

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"\nConfiguration file not found: {configPath}");
                continue;
            }

            return configPath;
        }
    }

    static Config ReadConfiguration(string configPath)
    {
        while (true)
        {
            try
            {
                string configJsonString = File.ReadAllText(configPath);
                Config? configurations = JsonSerializer.Deserialize<Config>(configJsonString);

                if (configurations is not null)
                    return configurations;
            }
            catch (Exception jsonException)
            {
                Console.WriteLine($"Exception reading configuration: {jsonException}");
                //throw;
            }

            Console.WriteLine("Configuration file is empty or invalid.");
            configPath = ReadConfigurationPath();
        }
    }

    static async Task SendEmailAsync(Config configurations, string type, string stock)
    {
        try
        {
            bool isSellType = type == "sell";
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(configurations.smtp.username);
                mail.Subject = isSellType ? "Alerta de venda de ações" : "Alerta de compra de ações";
                mail.Body = isSellType ? $"A ação {stock} está valorizada e deve ser vendida." : $"A ação {stock} está desvalorizada e deve ser comprada.";
                mail.IsBodyHtml = false;

                foreach (string email in configurations.recipientEmail)
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        mail.To.Add(email);
                    }
                }

                using (SmtpClient smtp = new SmtpClient(configurations.smtp.host, configurations.smtp.port ?? 587))
                {
                    smtp.Credentials = new NetworkCredential(configurations.smtp.username, configurations.smtp.password);
                    smtp.EnableSsl = configurations.smtp.enableSsl ?? true;
                    smtp.Timeout = 5000;

                    await smtp.SendMailAsync(mail);
                    Console.WriteLine("E-mail enviado com sucesso!");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
        }
    }
}

class EnvReader
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The file '{filePath}' does not exist.");

        foreach (string line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue; // Skip empty lines and comments

            string[] parts = line.Split('=', 2);
            if (parts.Length != 2)
                continue; // Skip lines that are not key-value pairs

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

class Config
{
    public required string[] recipientEmail { get; set; }
    public required Smtp smtp { get; set; }
}

class Smtp
{
    public required string host { get; set; }
    public int? port { get; set; }
    public bool? enableSsl { get; set; }
    public required string username { get; set; }
    public required string password { get; set; }
}

//reference: https://brapi.dev/docs/acoes/cotacao
class StockQuoteSuccessResponse
{
    public required StockQuoteSeries[] results { get; set; }
    public required string requestedAt { get; set; } //ISO 8601
    public required int took { get; set; }
}

class StockQuoteSeries
{
    public required string requestedSymbol { get; set; }
    public required string symbol { get; set; }
    public required bool changed { get; set; }
    public required StockQuoteSnapshot data { get; set; }
}

class StockQuoteSnapshot
{
    public required string shortName { get; set; }
    public required string longName { get; set; }
    public required string currency { get; set; }
    public required double regularMarketPrice { get; set; }
    public required double regularMarketDayHigh { get; set; }
    public required double regularMarketDayLow { get; set; }
    public required string regularMarketDayRange { get; set; }
    public required double regularMarketChange { get; set; }
    public required double regularMarketChangePercent { get; set; }
    public required string regularMarketTime { get; set; } //ISO 8601
    public required long marketCap { get; set; }
    public required long regularMarketVolume { get; set; }
    public required double regularMarketPreviousClose { get; set; }
    public required double regularMarketOpen { get; set; }
    public required string fiftyTwoWeekRange { get; set; }
    public required double fiftyTwoWeekLow { get; set; }
    public required double fiftyTwoWeekHigh { get; set; }
    public required string logourl { get; set; }
}

class StockQuoteErrorResponse
{
    public required bool error { get; set; }
    public required string message { get; set; }
    public string? code { get; set; }
}
