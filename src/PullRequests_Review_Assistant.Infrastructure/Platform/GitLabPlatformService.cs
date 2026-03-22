using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Infrastructure.Extensions;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Platform
{
    /// <summary>
    /// GitLab platform service using the MCP GitLab server
    /// to fetch merge request files and post review comments.
    /// </summary>
    public sealed class GitLabPlatformService : IRepositoryPlatformService, IAsyncDisposable
    {
        private const string PlatformName = "GitLab";
        private const string TokenEnvVar = "GITLAB_PERSONAL_ACCESS_TOKEN";

        private readonly IAuthStrategy _authStrategy;

        private IMcpClient? _mcpClient;  // TODO: Extract to parent

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabPlatformService"/> class.
        /// </summary>
        ///
        /// <param name="authStrategy">
        /// The authentication strategy used to resolve a GitLab personal access token.
        /// </param>
        public GitLabPlatformService(IAuthStrategy authStrategy)
        {
            _authStrategy = authStrategy;
        }

        /// <summary>
        /// Resolves the GitLab token via the auth strategy, sets it as the environment
        /// variable expected by the MCP server, then starts the MCP client.
        /// </summary>
        ///
        /// <param name="requiresTwoFactor">Whether two-factor authentication is required.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task InitializeAsync(bool requiresTwoFactor = false, CancellationToken cancellationToken = default)
        {
            var token = await _authStrategy.AuthenticateAsync(requiresTwoFactor, cancellationToken);

            // The MCP GitLab server reads the token from this environment variable
            Environment.SetEnvironmentVariable(TokenEnvVar, token);

            _mcpClient = await McpClientFactory.CreateAsync(
                new StdioClientTransport(
                    new StdioClientTransportOptions
                    {
                        Name = "GitLabMCP",
                        Command = "npx",
                        Arguments = ["-y", "@modelcontextprotocol/server-gitlab"],
                    }),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var result = await _mcpClient!.CallToolAsync("get_merge_request_changes", new Dictionary<string, object?>
            {
                ["project_id"] = $"{owner}/{repo}",
                ["merge_request_iid"] = pullRequestId
            }, cancellationToken: cancellationToken);

            List<PullRequestFile> files = [];

            files.AddRange(
                result.Content.OfType<TextContentBlock>()
                    .Select(content => new PullRequestFile
                    {
                        FilePath = content.Text,
                        DiffContent = content.Text,
                        Language = content.Text.InferLanguage()
                    }));

            return files;
        }

        /// <inheritdoc />
        public async Task PostReviewCommentAsync(
            string owner, string repo, int pullRequestId,
            ReviewComment comment, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            await _mcpClient!.CallToolAsync("create_merge_request_note", new Dictionary<string, object?>
            {
                ["project_id"] = $"{owner}/{repo}",
                ["merge_request_iid"] = pullRequestId,
                ["body"] = $"**[{comment.Severity}]** _{comment.ReviewArea}_ — `{comment.FilePath}:{comment.Line}`\n\n{comment.Body}"
            }, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()  // TODO: Extract to parent
        {
            if (_mcpClient is not null)
            {
                await _mcpClient.DisposeAsync();

                // Clear reference after disposal (prevents ObjectDisposedException on subsequent calls)
                _mcpClient = null;
            }
        }

        /// <summary>
        /// Ensures the MCP client is initialized before making API calls.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        private void EnsureInitialized()  // TODO: Extract to parent
        {
            if (_mcpClient is null)
            {
                throw new InvalidOperationException(
                    $"{PlatformName} MCP client not initialized. Call {nameof(InitializeAsync)} first.");
            }
        }
    }
}