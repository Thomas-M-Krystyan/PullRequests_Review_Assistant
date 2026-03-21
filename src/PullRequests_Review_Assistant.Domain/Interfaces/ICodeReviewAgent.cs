using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.ValueObjects;

namespace PullRequests_Review_Assistant.Domain.Interfaces
{
    /// <summary>
    /// The code review agent powered by GitHub Copilot SDK.
    /// 
    /// <para>
    /// Accepts a review configuration and produces review comments.
    /// </para>
    /// </summary>
    public interface ICodeReviewAgent
    {
        /// <summary>
        /// Reviews a single file according to the active configuration.
        /// </summary>
        /// 
        /// <param name="file">The file to review.</param>
        /// <param name="config">The review configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// 
        /// <returns>
        /// A list of review comments.
        /// </returns>
        public Task<IReadOnlyList<ReviewComment>> ReviewFileAsync(
            PullRequestFile file, ReviewConfiguration config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the system prompt at runtime (e.g., after language agent enrichment).
        /// </summary>
        /// 
        /// <param name="additionalPrompt">The additional prompt to include.</param>
        public void UpdateSystemPrompt(string additionalPrompt);
    }
}