using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Auth
{
    /// <summary>
    /// Factory that selects the correct <see cref="IAuthStrategy"/>
    /// based on the target <see cref="PlatformType"/>.
    /// </summary>
    public sealed class AuthStrategyFactory
    {
        private readonly ISecretsProvider _secrets;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthStrategyFactory"/> class.
        /// </summary>
        /// 
        /// <param name="secrets">The secrets.</param>
        public AuthStrategyFactory(ISecretsProvider secrets)
        {
            _secrets = secrets;
        }

        /// <summary>
        /// Creates the appropriate auth strategy for the given platform.
        /// </summary>
        /// 
        /// <param name="platform">The platform.</param>
        ///
        /// <returns>
        /// The authentication strategy for the specified platform.
        /// </returns>
        ///
        /// <exception cref="ArgumentException"/>
        public IAuthStrategy Create(PlatformType platform) => platform switch
        {
            PlatformType.GitHub => new GitHubAuthStrategy(_secrets),
            PlatformType.GitLab => new GitLabAuthStrategy(_secrets),
            PlatformType.Bitbucket => new BitbucketAuthStrategy(_secrets),

            // NOTE: Unsupported platforms should be caught at configuration time
            _ => throw new ArgumentException($"Unsupported platform: {platform}", nameof(platform))
        };
    }
}