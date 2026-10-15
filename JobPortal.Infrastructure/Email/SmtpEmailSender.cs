using JobPortal.Application.Interfaces.Infrastructure;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace JobPortal.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(EmailSettings settings) => _settings = settings;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(_settings.User, _settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}