using JobPortal.Application.DTO.Notifications;

namespace JobPortal.Application.Interfaces.Infrastructure;

public interface INotificationQueue
{
    void Enqueue(NotificationMessage message);
}