namespace PullRequests_Review_Assistant.Domain.Interfaces
{
    /// <summary>
    /// Abstraction over secret storage (e.g., Azure Key Vault).
    /// </summary>
    public interface ISecretsProvider
    {
        /// <summary>
        /// Retrieves a secret value by name.
        /// </summary>
        /// 
        /// <param name="secretName">The name of the secret to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// 
        /// <returns>
        ///   The secret value.
        /// </returns>
        public Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
    }
}