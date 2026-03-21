using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using PullRequests_Review_Assistant.Domain.Interfaces;

namespace PullRequests_Review_Assistant.Infrastructure.Secrets
{
    /// <summary>
    /// Azure Key Vault implementation of <see cref="ISecretsProvider"/>.
    ///
    /// <para>
    /// Uses <see cref="DefaultAzureCredential"/> for authentication.
    /// </para>
    /// </summary>
    public sealed class AzureKeyVaultSecretsProvider : ISecretsProvider
    {
        private readonly SecretClient _client;

        /// <summary>
        /// Initializes the provider with the Key Vault URI from
        /// environment variable <c>KEY_VAULT_NAME</c>.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public AzureKeyVaultSecretsProvider()
        {
            var vaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME")
                            ?? throw new InvalidOperationException(
                                "Environment variable KEY_VAULT_NAME is not set. " +
                                "Set it to your Azure Key Vault name (e.g., 'my-vault').");

            var vaultUri = new Uri($"https://{vaultName}.vault.azure.net");

            _client = new SecretClient(vaultUri, new DefaultAzureCredential());
        }

        /// <inheritdoc />
        public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);

            return response.Value.Value;
        }
    }
}