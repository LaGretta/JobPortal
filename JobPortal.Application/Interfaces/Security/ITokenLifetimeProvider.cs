namespace JobPortal.Application.Interfaces.Security;

public interface ITokenLifetimeProvider
{
    int AccessTokenMinutes { get; }
    int RefreshTokenDays { get; }
}