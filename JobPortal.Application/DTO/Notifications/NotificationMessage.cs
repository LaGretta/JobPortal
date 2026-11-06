using JobPortal.Application.DTO.Applications;

namespace JobPortal.Application.DTO.Notifications;

public enum NotificationType
{
    NewApplication = 0,
    ApplicationStatusChanged = 1
}

public record NotificationMessage(
    NotificationType Type,
    Guid TargetUserId,
    ApplicationDto Payload);