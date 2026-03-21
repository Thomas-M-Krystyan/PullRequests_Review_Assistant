using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Infrastructure.Extensions;

namespace PullRequests_Review_Assistant.Infrastructure.Platform
{
    /// <summary>
    /// GitHub platform service using the MCP GitHub server
    /// to fetch PR files and post review comments.
    /// </summary>
    public sealed class GitHubPlatformService : IRepositoryPlatformService, IAsyncDisposable
    {
        private const string PlatformName = "GitHub";

        private IMcpClient? _mcpClient;  // TODO: Extract to parent

        /// <summary>
        /// Initializes the MCP client connected to the GitHub MCP server.
        /// Requires GITHUB_PERSONAL_ACCESS_TOKEN environment variable.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _mcpClient = await McpClientFactory.CreateAsync(
                // The transport layer defines how the client communicates with the server
                new StdioClientTransport(
                    new StdioClientTransportOptions
                    {
                        Name = "GitHubMCP",
                        Command = "npx",
                        Arguments = ["-y", "@modelcontextprotocol/server-github"],
                    }),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var result = await _mcpClient!.CallToolAsync("get_pull_request_files", new Dictionary<string, object?>
            {
                ["owner"] = owner,
                ["repo"] = repo,
                ["pull_number"] = pullRequestId
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

            await _mcpClient!.CallToolAsync("create_pull_request_review", new Dictionary<string, object?>
            {
                ["owner"] = owner,
                ["repo"] = repo,
                ["pull_number"] = pullRequestId,
                ["body"] = $"[{comment.Severity.ToUpperInvariant()}] ({comment.ReviewArea}) {comment.Body}",
                ["event"] = "COMMENT",
                ["comments"] = new[]
                {
                    new
                    {
                        path = comment.FilePath,
                        line = comment.Line,
                        body = $"**[{comment.Severity}]** _{comment.ReviewArea}_\n\n{comment.Body}"
                    }
                }
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
                throw new InvalidOperationException($"{PlatformName} MCP client not initialized. Call {nameof(InitializeAsync)} first.");
            }
        }
    }
}