using PullRequests_Review_Assistant.Domain.Interfaces;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Auth
{
    /// <summary>
    /// GitLab authentication strategy.
    /// Uses a personal access token from Azure Key Vault.
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
        public async Task<string> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            var token = await _secrets.GetSecretAsync("gitlab-pat", cancellationToken);

            Console.WriteLine("[GitLab Auth] Authenticated successfully.");
            Console.WriteLine();

            return token;
        }
    }
}