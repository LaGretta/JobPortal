using System.Threading.Channels;
using JobPortal.Application.DTO.Notifications;
using JobPortal.Application.Interfaces.Infrastructure;

namespace JobPortal.Api.Services;

public class NotificationQueue : INotificationQueue
{
    private readonly Channel<NotificationMessage> _channel;
    private readonly ILogger<NotificationQueue> _logger;

    public NotificationQueue(ILogger<NotificationQueue> logger)
    {
        _logger = logger;
        
        _channel = Channel.CreateBounded<NotificationMessage>(
            new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true
            });
    }

    public void Enqueue(NotificationMessage message)
    {
        if (!_channel.Writer.TryWrite(message))
            _logger.LogWarning("Черга сповіщень переповнена, повідомлення відкинуто");
    }

    public ChannelReader<NotificationMessage> Reader => _channel.Reader;
}