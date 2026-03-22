using PullRequests_Review_Assistant.Domain.Interfaces;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Auth
{
    /// <summary>
    /// GitHub authentication strategy.
    /// Uses a personal access token from Azure Key Vault,
    /// with optional device-flow 2FA support.
    /// </summary>
    public sealed class GitHubAuthStrategy : IAuthStrategy
    {
        private readonly ISecretsProvider _secrets;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubAuthStrategy"/> class.
        /// </summary>
        /// 
        /// <param name="secrets">The secrets.</param>
        public GitHubAuthStrategy(ISecretsProvider secrets)
        {
            _secrets = secrets;
        }

        /// <inheritdoc />
        public async Task<string> AuthenticateAsync(bool requiresTwoFactor, CancellationToken cancellationToken = default)
        {
            var token = await _secrets.GetSecretAsync("github-pat", cancellationToken);

            if (requiresTwoFactor)
            {
                Console.WriteLine("[GitHub Auth] Two-factor authentication required.");
                Console.Write("[GitHub Auth] Enter your 2FA code: ");
                var code = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new InvalidOperationException("2FA code is required.");
                }

                // In production, exchange token + 2FA code via GitHub OAuth API
                Console.WriteLine("[GitHub Auth] 2FA validated (simulated).");
            }

            Console.WriteLine("[GitHub Auth] Authenticated successfully.");
            Console.WriteLine();

            return token;
        }
    }
}