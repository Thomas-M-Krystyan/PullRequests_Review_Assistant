namespace PullRequests_Review_Assistant.Domain.Interfaces
{
    /// <summary>
    /// Strategy interface for platform authentication.
    /// 
    /// <para>
    ///   Each hosting platform implements its own authentication flow,
    ///   optionally supporting second-step (2FA / OAuth device flow) authorization.
    /// </para>
    /// </summary>
    public interface IAuthStrategy
    {
        /// <summary>
        /// Authenticates and returns an access token.
        /// </summary>
        /// 
        /// <param name="requiresTwoFactor">Whether 2FA is required.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// 
        /// <returns>
        /// A valid access token string.
        /// </returns>
        Task<string> AuthenticateAsync(bool requiresTwoFactor, CancellationToken cancellationToken = default);
    }
}