using ModelContextProtocol.Client;
using PullRequests_Review_Assistant.Domain.Interfaces;

namespace PullRequests_Review_Assistant.Infrastructure.Platform.Parent
{
    /// <summary>
    /// Base class for MCP-backed platform services.
    ///
    /// <para>
    /// Owns the shared <see cref="IMcpClient"/> lifetime — initialization via
    /// <see cref="SetMcpClient"/>, guarded access via <see cref="McpClient"/>,
    /// and async teardown via <see cref="DisposeAsync"/>.
    /// </para>
    /// </summary>
    public abstract class McpPlatformServiceBase : IRepositoryPlatformService
    {
        private IMcpClient? _mcpClient;

        /// <summary>
        /// Provides guarded access to the MCP client for use in subclass operations.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        ///   Thrown if accessed before <see cref="SetMcpClient"/> has been called.
        /// </exception>
        protected IMcpClient McpClient
        {
            get
            {
                EnsureInitialized();

                return _mcpClient!;
            }
        }

        /// <inheritdoc />
        public abstract Task InitializeAsync(bool requiresTwoFactor = false, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task<IReadOnlyList<Domain.Entities.PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task PostReviewCommentAsync(
            string owner, string repo, int pullRequestId,
            Domain.Entities.ReviewComment comment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores the fully constructed MCP client. Called once from <see cref="InitializeAsync"/>.
        /// </summary>
        ///
        /// <param name="client">The initialised MCP client.</param>
        protected void SetMcpClient(IMcpClient client)
        {
            _mcpClient = client;
        }

        /// <summary>
        /// Ensures the MCP client has been set before making API calls.
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException"/>
        private void EnsureInitialized()
        {
            if (_mcpClient is null)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} MCP client not initialized. Call {nameof(InitializeAsync)} first.");
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_mcpClient is not null)
            {
                await _mcpClient.DisposeAsync();

                // Clear reference after disposal (prevents ObjectDisposedException on subsequent calls)
                _mcpClient = null;
            }
        }
    }
}