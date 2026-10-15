namespace JobPortal.Infrastructure.Email;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = "JobPortal";
    public string FromAddress { get; set; } = string.Empty;

    // Базова адреса фронту — на неї вказує посилання в листі.
    public string ResetUrlBase { get; set; } = string.Empty;
}