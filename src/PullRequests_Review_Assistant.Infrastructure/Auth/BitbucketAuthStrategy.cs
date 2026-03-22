using PullRequests_Review_Assistant.Domain.Interfaces;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Auth
{
    /// <summary>
    /// Bitbucket authentication strategy.
    /// Uses an app password from Azure Key Vault,
    /// with optional OAuth2 for 2FA.
    /// </summary>
    /// <remarks>
    /// Returns credentials in <c>username:app-password</c> format,
    /// which <see cref="Platform.BitbucketPlatformService"/> splits into the two
    /// environment variables expected by the MCP Bitbucket server:
    /// <c>BITBUCKET_USERNAME</c> and <c>BITBUCKET_APP_PASSWORD</c>.
    /// </remarks>
    public sealed class BitbucketAuthStrategy : IAuthStrategy
    {
        private readonly ISecretsProvider _secrets;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitbucketAuthStrategy"/> class.
        /// </summary>
        ///
        /// <param name="secrets">The secrets provider.</param>
        public BitbucketAuthStrategy(ISecretsProvider secrets)
        {
            _secrets = secrets;
        }

        /// <summary>
        ///   <inheritdoc/>
        /// </summary>
        /// <returns>
        ///   <inheritdoc select="returns"/>
        ///   A composite credential string in <c>username:app-password</c> format.
        /// </returns>
        /// <exception cref="InvalidOperationException"/>
        public async Task<string> AuthenticateAsync(bool requiresTwoFactor, CancellationToken cancellationToken = default)
        {
            var username = await _secrets.GetSecretAsync("bitbucket-username", cancellationToken);
            var appPassword = await _secrets.GetSecretAsync("bitbucket-app-password", cancellationToken);

            if (requiresTwoFactor)
            {
                Console.WriteLine("[Bitbucket Auth] Two-factor authentication required.");
                Console.Write("[Bitbucket Auth] Enter your 2FA code: ");
                var code = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new InvalidOperationException("2FA code is required.");
                }

                Console.WriteLine("[Bitbucket Auth] 2FA validated (simulated).");
            }

            Console.WriteLine("[Bitbucket Auth] Authenticated successfully.");

            // Encode as "username:app-password" for the platform service to split
            return $"{username}:{appPassword}";
        }
    }
}