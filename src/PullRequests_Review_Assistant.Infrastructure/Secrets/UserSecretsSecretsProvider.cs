using Microsoft.Extensions.Configuration;
using PullRequests_Review_Assistant.Domain.Interfaces;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Secrets
{
    /// <summary>
    /// User Secrets implementation of <see cref="ISecretsProvider"/>.
    ///
    /// <para>
    /// Reads secrets from the .NET User Secrets store (<c>secrets.json</c>),
    /// which is kept outside the repository and is never committed to source control.
    /// Intended for local development only.
    /// </para>
    /// </summary>
    public sealed class UserSecretsSecretsProvider : ISecretsProvider
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSecretsSecretsProvider"/> class.
        /// </summary>
        ///
        /// <param name="userSecretsId">
        /// The User Secrets ID declared in the Console project's <c>.csproj</c> file.
        /// </param>
        public UserSecretsSecretsProvider(string userSecretsId)
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets(userSecretsId)
                .Build();
        }

        /// <inheritdoc />
        /// <exception cref="KeyNotFoundException"/>
        public Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
        {
            var value = _configuration[secretName]
                        ?? throw new KeyNotFoundException(
                            $"Secret '{secretName}' was not found in User Secrets. " +
                            $"Run: dotnet user-secrets set \"{secretName}\" \"<value>\"");

            return Task.FromResult(value);
        }
    }
}