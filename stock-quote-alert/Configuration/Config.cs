namespace Configuration;

class Config
{
    public required string[] RecipientEmail { get; set; }
    public required Smtp Smtp { get; set; }
}

class Smtp
{
    public required string Host { get; set; }
    public int? Port { get; set; }
    public bool? EnableSsl { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}