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
    /// GitLab platform service using the MCP GitLab server
    /// to fetch merge request files and post review comments.
    /// </summary>
    public sealed class GitLabPlatformService : McpPlatformServiceBase
    {
        private const string TokenEnvVar = "GITLAB_PERSONAL_ACCESS_TOKEN";

        private readonly IAuthStrategy _authStrategy;

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

        /// <inheritdoc />
        public override async Task InitializeAsync(bool requiresTwoFactor = false, CancellationToken cancellationToken = default)
        {
            var token = await _authStrategy.AuthenticateAsync(requiresTwoFactor, cancellationToken);

            // The MCP GitLab server reads the token from this environment variable
            Environment.SetEnvironmentVariable(TokenEnvVar, token);

            var mcpClient = await McpClientFactory.CreateAsync(
                new StdioClientTransport(
                    new StdioClientTransportOptions
                    {
                        Name = "GitLabMCP",
                        Command = "npx",
                        Arguments = ["-y", "@modelcontextprotocol/server-gitlab"],
                    }),
                cancellationToken: cancellationToken);

            SetMcpClient(mcpClient);
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default)
        {
            var result = await McpClient.CallToolAsync("get_merge_request_changes", new Dictionary<string, object?>
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
        public override async Task PostReviewCommentAsync(
            string owner, string repo, int pullRequestId,
            ReviewComment comment, CancellationToken cancellationToken = default)
        {
            await McpClient.CallToolAsync("create_merge_request_note", new Dictionary<string, object?>
            {
                ["project_id"] = $"{owner}/{repo}",
                ["merge_request_iid"] = pullRequestId,
                ["body"] = $"**[{comment.Severity}]** _{comment.ReviewArea}_ — `{comment.FilePath}:{comment.Line}`\n\n{comment.Body}"
            }, cancellationToken: cancellationToken);
        }
    }
}