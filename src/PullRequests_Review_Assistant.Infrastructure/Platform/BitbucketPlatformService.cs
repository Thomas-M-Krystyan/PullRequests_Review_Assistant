using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Infrastructure.Extensions;

namespace PullRequests_Review_Assistant.Infrastructure.Platform
{
    /// <summary>
    /// Bitbucket platform service using the MCP Bitbucket server
    /// to fetch PR files and post review comments.
    /// </summary>
    public sealed class BitbucketPlatformService : IRepositoryPlatformService, IAsyncDisposable
    {
        private const string PlatformName = "BitBucket";

        private IMcpClient? _mcpClient;  // TODO: Extract to parent

        /// <summary>
        /// Initializes the MCP client connected to the Bitbucket MCP server.
        /// Requires BITBUCKET_APP_PASSWORD environment variable.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _mcpClient = await McpClientFactory.CreateAsync(
                new StdioClientTransport(
                    new StdioClientTransportOptions
                    {
                        Name = "BitbucketMCP",
                        Command = "npx",
                        Arguments = ["-y", "@modelcontextprotocol/server-bitbucket"],
                    }),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var result = await _mcpClient!.CallToolAsync("get_pull_request_diff", new Dictionary<string, object?>
            {
                ["workspace"] = owner,
                ["repo_slug"] = repo,
                ["pull_request_id"] = pullRequestId
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

            await _mcpClient!.CallToolAsync("create_pull_request_comment", new Dictionary<string, object?>
            {
                ["workspace"] = owner,
                ["repo_slug"] = repo,
                ["pull_request_id"] = pullRequestId,
                ["body"] = $"**[{comment.Severity}]** _{comment.ReviewArea}_ — `{comment.FilePath}:{comment.Line}`\n\n{comment.Body}",
                ["inline"] = new { path = comment.FilePath, to = comment.Line }
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