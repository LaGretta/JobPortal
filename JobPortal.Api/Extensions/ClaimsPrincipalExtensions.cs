using System.Security.Claims;

namespace JobPortal.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        string? id = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(id, out var guid)
            ? guid
            : throw new UnauthorizedAccessException("Токен без ідентифікатора користувача");
    }
}
