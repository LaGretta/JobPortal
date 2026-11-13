using JobPortal.Application.Interfaces.Infrastructure;

namespace JobPortal.Tests.Fakes;

public class FakeEmailSender : IEmailSender
{
    public record SentEmail(string To, string Subject);

    private readonly List<SentEmail> _sent = new();

    public string? LastResetToken { get; private set; }

    public IReadOnlyList<SentEmail> Sent
    {
        get { lock (_sent) return _sent.ToList(); }
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        lock (_sent) _sent.Add(new SentEmail(to, subject));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string to, string firstName, string token, CancellationToken ct)
    {
        LastResetToken = token;
        lock (_sent) _sent.Add(new SentEmail(to, "password-reset"));
        return Task.CompletedTask;
    }
}