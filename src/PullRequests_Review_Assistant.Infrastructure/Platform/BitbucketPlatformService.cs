using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Infrastructure.Extensions;
using PullRequests_Review_Assistant.Infrastructure.Platform.Parent;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Platform
{
    /// <summary>
    /// Bitbucket platform service using the MCP Bitbucket server
    /// to fetch PR files and post review comments.
    /// </summary>
    public sealed class BitbucketPlatformService : McpPlatformServiceBase
    {
        private const string UsernameEnvVar = "BITBUCKET_USERNAME";
        private const string AppPasswordEnvVar = "BITBUCKET_APP_PASSWORD";

        private readonly IAuthStrategy _authStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitbucketPlatformService"/> class.
        /// </summary>
        ///
        /// <param name="authStrategy">
        /// The authentication strategy used to resolve Bitbucket credentials.
        /// Must return credentials in <c>username:app-password</c> format.
        /// </param>
        public BitbucketPlatformService(IAuthStrategy authStrategy)
        {
            _authStrategy = authStrategy;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"/>
        public override async Task InitializeAsync(bool requiresTwoFactor = false, CancellationToken cancellationToken = default)
        {
            var credentials = await _authStrategy.AuthenticateAsync(requiresTwoFactor, cancellationToken);

            // BitbucketAuthStrategy returns "username:app-password" — split on first colon only
            var separatorIndex = credentials.IndexOf(':', StringComparison.Ordinal);

            if (separatorIndex < 0)
            {
                throw new ArgumentException(
                    "Bitbucket credentials must be in 'username:app-password' format.");
            }

            var username = credentials[..separatorIndex];
            var appPassword = credentials[(separatorIndex + 1)..];

            // The MCP Bitbucket server reads credentials from these environment variables
            Environment.SetEnvironmentVariable(UsernameEnvVar, username);
            Environment.SetEnvironmentVariable(AppPasswordEnvVar, appPassword);

            var transport = new StdioClientTransport(
                new StdioClientTransportOptions
                {
                    Name = "BitbucketMCP",
                    Command = "npx",
                    Arguments = ["-y", "@modelcontextprotocol/server-bitbucket"],
                });

            var mcpClient = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

            SetMcpClient(mcpClient);
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default)
        {
            var result = await McpClient.CallToolAsync("get_pull_request_diff", new Dictionary<string, object?>
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
        public override async Task PostReviewCommentAsync(
            string owner, string repo, int pullRequestId,
            ReviewComment comment, CancellationToken cancellationToken = default)
        {
            await McpClient.CallToolAsync("create_pull_request_comment", new Dictionary<string, object?>
            {
                ["workspace"] = owner,
                ["repo_slug"] = repo,
                ["pull_request_id"] = pullRequestId,
                ["body"] = $"**[{comment.Severity}]** _{comment.ReviewArea}_ — `{comment.FilePath}:{comment.Line}`\n\n{comment.Body}",
                ["inline"] = new { path = comment.FilePath, to = comment.Line }
            }, cancellationToken: cancellationToken);
        }
    }
}