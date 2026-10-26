using JobPortal.Application.DTO.Applications;

namespace JobPortal.Application.Interfaces.Infrastructure;

public interface IApplicationNotifier
{
    //SignalR
    Task NotifyNewApplicationAsync(Guid employerUserId, ApplicationDto application, CancellationToken ct);
    Task NotifyStatusChangedAsync(Guid candidateUserId, ApplicationDto application, CancellationToken ct);
}