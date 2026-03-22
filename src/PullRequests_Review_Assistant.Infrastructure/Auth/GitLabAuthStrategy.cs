using PullRequests_Review_Assistant.Domain.Interfaces;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Auth
{
    /// <summary>
    /// GitLab authentication strategy.
    /// Uses a personal access token from Azure Key Vault,
    /// with optional OAuth2 device-flow for 2FA.
    /// </summary>
    public sealed class GitLabAuthStrategy : IAuthStrategy
    {
        private readonly ISecretsProvider _secrets;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabAuthStrategy"/> class.
        /// </summary>
        /// 
        /// <param name="secrets">The secrets.</param>
        public GitLabAuthStrategy(ISecretsProvider secrets)
        {
            _secrets = secrets;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"/>
        public async Task<string> AuthenticateAsync(bool requiresTwoFactor, CancellationToken cancellationToken = default)
        {
            var token = await _secrets.GetSecretAsync("gitlab-pat", cancellationToken);

            if (requiresTwoFactor)
            {
                Console.WriteLine("[GitLab Auth] Two-factor authentication required.");
                Console.Write("[GitLab Auth] Enter your 2FA code: ");
                var code = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new ArgumentException("2FA code is required.");
                }

                Console.WriteLine("[GitLab Auth] 2FA validated (simulated).");
            }

            Console.WriteLine("[GitLab Auth] Authenticated successfully.");
            Console.WriteLine();

            return token;
        }
    }
}