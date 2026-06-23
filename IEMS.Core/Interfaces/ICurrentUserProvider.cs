namespace IEMS.Core.Interfaces;

/// <summary>
/// Supplies the username of the currently signed-in user to lower layers (e.g. the audit
/// interceptor) without those layers depending on the WPF/session code that tracks login.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>The signed-in username, or "system" when no user is signed in.</summary>
    string UserName { get; }
}
