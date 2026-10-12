using System.Text.Json;

namespace Configuration;

static class ConfigurationLoader
{
    public static Config ReadConfiguration(string configPath)
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
            }

            Console.WriteLine("Configuration file is empty or invalid.");
            configPath = ReadConfigurationPath();
        }
    }

    public static string ReadConfigurationPath()
    {
        while (true)
        {
            Console.WriteLine("Enter the path to the JSON configuration file. Type 'help' to see expected file.\n");
            string? configPath = Console.ReadLine()?.Trim();

            if (configPath == "help")
            {
                Console.WriteLine("\n =========== FILE EXAMPLE ===========");
                Console.WriteLine("{");
                Console.WriteLine("   RecipientEmail: string[],");
                Console.WriteLine("   Smtp: {");
                Console.WriteLine("      Host: string");
                Console.WriteLine("      Port: int?");
                Console.WriteLine("      EnableSsl: bool?");
                Console.WriteLine("      Username: string");
                Console.WriteLine("      Password: string");
                Console.WriteLine("   }");
                Console.WriteLine("}");
                Console.WriteLine("======================================\n");
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
}
