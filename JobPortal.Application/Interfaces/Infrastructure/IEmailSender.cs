namespace JobPortal.Application.Interfaces.Infrastructure;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct);
}