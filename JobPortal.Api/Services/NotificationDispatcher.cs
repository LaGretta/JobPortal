using JobPortal.Application.DTO.Notifications;
using JobPortal.Application.Interfaces.Infrastructure;

namespace JobPortal.Api.Services;

public class NotificationDispatcher : BackgroundService
{
    private readonly NotificationQueue _queue;
    private readonly IApplicationNotifier _notifier;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        NotificationQueue queue,
        IApplicationNotifier notifier,
        ILogger<NotificationDispatcher> logger)
    {
        _queue = queue;
        _notifier = notifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var message in _queue.Reader.ReadAllAsync(ct))
        {
            try
            {
                switch (message.Type)
                {
                    case NotificationType.NewApplication:
                        await _notifier.NotifyNewApplicationAsync(
                            message.TargetUserId, message.Payload, ct);
                        break;

                    case NotificationType.ApplicationStatusChanged:
                        await _notifier.NotifyStatusChangedAsync(
                            message.TargetUserId, message.Payload, ct);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не вдалось доставити сповіщення {Type} для {User}",
                    message.Type, message.TargetUserId);
            }
        }
    }
}