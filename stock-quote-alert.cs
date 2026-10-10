#:property PublishAot=false
#:property JsonSerializerIsReflectionEnabledByDefault=true

using System;
using System.Text.Json;
using System.Net;
using System.Net.Mail;

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

        string configPath = ReadConfigurationPath();
        Console.WriteLine($"Using configuration file: {configPath}");

        Config configurations = ReadConfiguration(configPath);

        Console.WriteLine($"recipientEmail: {string.Join(", ", configurations.recipientEmail)}");
        Console.WriteLine($"smtp host: {configurations.smtp.host}");
        Console.WriteLine($"smtp port: {configurations.smtp.port}");
        Console.WriteLine($"smtp ssl: {configurations.smtp.enableSsl}");
        Console.WriteLine($"smtp username: {configurations.smtp.username}");

        SendEmail(configurations);
    }

    static string ReadConfigurationPath()
    {
        while (true)
        {
            Console.WriteLine("Enter the path to the JSON configuration file. Type 'help' to see expected file.");
            string? configPath = Console.ReadLine()?.Trim();

            if (configPath == "help") {
                Console.WriteLine("File must have recipientEmail (string[]) and smtp ({host: string, port: int?, enableSsl: boolean?, username: string, password: string}) properties.");
                continue;
            }

            if (string.IsNullOrEmpty(configPath))
            {
                Console.WriteLine("Enter a file path.");
                continue;
            }

            if (!configPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Configuration file must be a JSON file.");
                continue;
            }

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Configuration file not found: {configPath}");
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

    static void SendEmail(Config configurations) {
        try
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(configurations.smtp.username);
                mail.Subject = "Teste";
                mail.Body = "Olá, este é um e-mail de teste enviado automaticamente via C# .NET.";
                mail.IsBodyHtml = false;

                // 2. Configurar o(s) destinatário(s)
                foreach (string email in configurations.recipientEmail) {
                    if (!string.IsNullOrWhiteSpace(email)) {
                        mail.To.Add(email);
                    }
                }

                using (SmtpClient smtp = new SmtpClient(configurations.smtp.host, configurations.smtp.port ?? 587))
                {
                    smtp.Credentials = new NetworkCredential(configurations.smtp.username, configurations.smtp.password);
                    smtp.EnableSsl = configurations.smtp.enableSsl ?? true;
                    smtp.Timeout = 20000;

                    smtp.Send(mail);
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