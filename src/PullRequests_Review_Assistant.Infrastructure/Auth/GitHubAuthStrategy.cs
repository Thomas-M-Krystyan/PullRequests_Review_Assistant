using PullRequests_Review_Assistant.Domain.Interfaces;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Auth
{
    /// <summary>
    /// GitHub authentication strategy.
    /// Uses a personal access token from Azure Key Vault.
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
        public async Task<string> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            var token = await _secrets.GetSecretAsync("github-pat", cancellationToken);

            Console.WriteLine("[GitHub Auth] Authenticated successfully.");
            Console.WriteLine();

            return token;
        }
    }
}