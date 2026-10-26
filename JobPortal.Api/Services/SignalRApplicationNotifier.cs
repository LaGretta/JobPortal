using JobPortal.Api.Hubs;
using JobPortal.Application.DTO.Applications;
using JobPortal.Application.Interfaces.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace JobPortal.Api.Services;

public class SignalRApplicationNotifier : IApplicationNotifier
{
    private readonly IHubContext<ApplicationsHub> _hub;

    public SignalRApplicationNotifier(IHubContext<ApplicationsHub> hub) => _hub = hub;

    public Task NotifyNewApplicationAsync(
        Guid employerUserId, ApplicationDto application, CancellationToken ct)
        => _hub.Clients
            .User(employerUserId.ToString())
            .SendAsync("NewApplication", application, ct);

    public Task NotifyStatusChangedAsync(
        Guid candidateUserId, ApplicationDto application, CancellationToken ct)
        => _hub.Clients
            .User(candidateUserId.ToString())
            .SendAsync("ApplicationStatusChanged", application, ct);
}