using IEMS.Core.Interfaces;

namespace IEMS.WPF.Services;

/// <summary>
/// Bridges the WPF login session to the lower layers: reports the username of whoever is signed in
/// so the audit interceptor can attribute changes. Falls back to "system" when no one is signed in.
/// </summary>
public class CurrentUserProvider : ICurrentUserProvider
{
    public string UserName => LoginWindow.CurrentUser?.Username ?? "system";
}
