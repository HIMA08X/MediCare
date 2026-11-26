namespace MediCare.API.Options;

public sealed class AdminSeedOptions
{
    public bool Enabled { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
