using PullRequests_Review_Assistant.Domain.Entities;

namespace PullRequests_Review_Assistant.Domain.Interfaces
{
    /// <summary>
    /// Abstraction for interacting with a repository hosting platform's pull request API.
    /// 
    /// <para>
    /// Implemented per platform (GitHub, GitLab, Bitbucket) via MCP servers.
    /// </para>
    /// </summary>
    public interface IRepositoryPlatformService
    {
        /// <summary>
        /// Authenticates with the platform and starts the underlying MCP client.
        /// Must be called once before any other method on this service.
        /// </summary>
        ///
        /// <param name="requiresTwoFactor">Whether two-factor authentication is required.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task InitializeAsync(bool requiresTwoFactor = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches all changed files in the pull request.
        /// </summary>
        /// 
        /// <param name="owner">The repository owner.</param>
        /// <param name="repo">The repository name.</param>
        /// <param name="pullRequestId">The pull request identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// 
        /// <returns>
        /// A list of files changed in the pull request.
        /// </returns>
        public Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Posts a review comment on a specific file and line.
        /// </summary>
        /// 
        /// <param name="owner">The repository owner.</param>
        /// <param name="repo">The repository name.</param>
        /// <param name="pullRequestId">The pull request identifier.</param>
        /// <param name="comment">The review comment to post.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task PostReviewCommentAsync(  // TODO: result + created comment's ID for future reference (e.g., updates/deletes)
            string owner, string repo, int pullRequestId,
            ReviewComment comment, CancellationToken cancellationToken = default);
    }
}