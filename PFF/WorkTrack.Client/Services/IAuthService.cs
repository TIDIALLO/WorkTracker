namespace WorkTrack.Client.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}
