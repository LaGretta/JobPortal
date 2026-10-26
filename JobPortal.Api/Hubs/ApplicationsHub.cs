using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JobPortal.Api.Hubs;

[Authorize]
public class ApplicationsHub : Hub
{
    // Порожній навмисно: клієнт нічого не викликає, тільки слухає.
    // Групи теж не потрібні — SignalR сам веде мапу "юзер → з'єднання"
    // за ClaimTypes.NameIdentifier з токена.
}