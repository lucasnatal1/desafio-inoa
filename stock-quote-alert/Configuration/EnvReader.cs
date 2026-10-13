namespace Configuration;

static class EnvReader
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (string line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            string[] parts = line.Split('=', 2);
            if (parts.Length != 2)
                continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}