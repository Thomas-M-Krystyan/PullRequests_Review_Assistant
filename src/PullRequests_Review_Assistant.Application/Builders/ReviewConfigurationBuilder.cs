using PullRequests_Review_Assistant.Application.Builders.Interface;
using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.ValueObjects;

namespace PullRequests_Review_Assistant.Application.Builders
{
    /// <summary>
    /// Concrete fluent builder for <see cref="ReviewConfiguration"/>.
    /// </summary>
    ///
    /// <remarks>
    /// Core areas (Performance, Architecture, Vulnerabilities, CodeSmells)
    /// are always included. Extended areas are opt-in.
    /// </remarks>
    public sealed class ReviewConfigurationBuilder : IReviewConfigurationBuilder
    {
        // Core review area
        private ReviewArea _areas = ReviewArea.CoreReview;

        // Platform and repository details
        private PlatformType _platform;
        private string _repoOwner = string.Empty;
        private string _repoName = string.Empty;
        private int _pullRequestId;

        // Language-specific standards
        private string _language = string.Empty;

        /// <inheritdoc />
        public IReviewConfigurationBuilder ForPlatform(PlatformType platform)
        {
            _platform = platform;

            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder ForRepository(string repositoryOwner, string repositoryName)
        {
            _repoOwner = repositoryOwner;
            _repoName = repositoryName;

            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder ForPullRequest(int pullRequestId)
        {
            _pullRequestId = pullRequestId;

            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder WithLanguage(string language)
        {
            _language = language;

            return this;
        }

        #region Additional Review Areas
        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeCodeFormatting()
        {
            _areas |= ReviewArea.CodeFormatting;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeLinting()
        {
            _areas |= ReviewArea.Linting;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeCopyrights()
        {
            _areas |= ReviewArea.Copyrights;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeDocumentation()
        {
            _areas |= ReviewArea.Documentation;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeNaming()
        {
            _areas |= ReviewArea.Naming;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeErrorHandling()
        {
            _areas |= ReviewArea.ErrorHandling;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeConcurrency()
        {
            _areas |= ReviewArea.Concurrency;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeTesting()
        {
            _areas |= ReviewArea.Testing;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeDependencyManagement()
        {
            _areas |= ReviewArea.DependencyManagement;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeAccessibility()
        {
            _areas |= ReviewArea.Accessibility;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeLogging()
        {
            _areas |= ReviewArea.Logging;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeHardcodedSecrets()
        {
            _areas |= ReviewArea.HardcodedSecrets;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeDeadCode()
        {
            _areas |= ReviewArea.DeadCode;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeComplexity()
        {
            _areas |= ReviewArea.Complexity;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeDuplicateCode()
        {
            _areas |= ReviewArea.DuplicateCode;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeApiDesign()
        {
            _areas |= ReviewArea.ApiDesign;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeArea(ReviewArea area)
        {
            _areas |= area;
            return this;
        }

        /// <inheritdoc />
        public IReviewConfigurationBuilder IncludeAll()
        {
            _areas = ReviewArea.All;
            return this;
        }
        #endregion

        /// <inheritdoc />
        /// <exception cref="ArgumentException"/>
        public ReviewConfiguration Build()
        {
            return string.IsNullOrWhiteSpace(_repoOwner)
                ? throw new ArgumentException("Repository owner is required.")
                : string.IsNullOrWhiteSpace(_repoName)

                    ? throw new ArgumentException("Repository name is required.")
                    : _pullRequestId <= 0

                        ? throw new ArgumentException("A valid pull request ID is required.")
                        : new ReviewConfiguration
                        {
                            Areas = _areas,
                            Platform = _platform,
                            RepositoryOwner = _repoOwner,
                            RepositoryName = _repoName,
                            PullRequestId = _pullRequestId,
                            TargetLanguage = _language
                        };
        }
    }
}