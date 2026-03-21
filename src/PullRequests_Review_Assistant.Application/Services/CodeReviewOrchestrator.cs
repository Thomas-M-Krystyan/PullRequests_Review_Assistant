using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Domain.ValueObjects;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Application.Services
{
    /// <summary>
    /// Orchestrates the full code review pipeline:
    /// 
    /// <list type="number">
    ///   <item>Optionally enrich system prompt via the language agent.</item>
    ///   <item>Fetch PR files from the repository platform.</item>
    ///   <item>Review each file using the code review agent.</item>
    ///   <item>Post comments back to the platform.</item>
    /// </list>
    /// </summary>
    public sealed class CodeReviewOrchestrator
    {
        private readonly ICodeReviewAgent _codeReviewAgent;
        private readonly ILanguageAgent _languageAgent;
        private readonly IRepositoryPlatformService _platformService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeReviewOrchestrator"/> class.
        /// </summary>
        /// 
        /// <param name="codeReviewAgent">The code review agent.</param>
        /// <param name="languageAgent">The language agent.</param>
        /// <param name="platformService">The platform service.</param>
        public CodeReviewOrchestrator(
            ICodeReviewAgent codeReviewAgent,
            ILanguageAgent languageAgent,
            IRepositoryPlatformService platformService)
        {
            _codeReviewAgent = codeReviewAgent;
            _languageAgent = languageAgent;
            _platformService = platformService;
        }

        /// <summary>
        /// Executes a complete code review session for the given configuration.
        /// </summary>
        /// 
        /// <param name="config">The review configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// 
        /// <returns>
        /// A list of review comments.
        /// </returns>
        public async Task<IReadOnlyList<ReviewComment>> RunReviewAsync(
            ReviewConfiguration config, CancellationToken cancellationToken = default)
        {
            // Step 1: Language-specific enrichment
            if (!string.IsNullOrWhiteSpace(config.TargetLanguage))
            {
                var languagePrompt = await _languageAgent
                    .GetLanguageStandardsPromptAsync(config.TargetLanguage, cancellationToken);

                _codeReviewAgent.UpdateSystemPrompt(languagePrompt);

                Console.WriteLine($"[Orchestrator] Language standards loaded for {config.TargetLanguage}.");
            }

            // Step 2: Fetch PR files
            var files = await _platformService.GetPullRequestFilesAsync(
                config.RepositoryOwner, config.RepositoryName,
                config.PullRequestId, cancellationToken);

            Console.WriteLine($"[Orchestrator] Found {files.Count} file(s) to review.");

            // Step 3: Review each file
            var allComments = new List<ReviewComment>();

            foreach (var file in files)
            {
                Console.WriteLine($"[Orchestrator] Reviewing: {file.FilePath}");

                var comments = await _codeReviewAgent.ReviewFileAsync(file, config, cancellationToken);
                allComments.AddRange(comments);
            }

            Console.WriteLine($"[Orchestrator] Total comments generated: {allComments.Count}");

            // Step 4: Post comments back to the platform
            foreach (var comment in allComments)
            {
                await _platformService.PostReviewCommentAsync(
                    config.RepositoryOwner, config.RepositoryName,
                    config.PullRequestId, comment, cancellationToken);
            }

            Console.WriteLine("[Orchestrator] All comments posted successfully.");

            return allComments;
        }
    }
}