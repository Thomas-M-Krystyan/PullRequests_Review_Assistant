using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.ValueObjects;

namespace PullRequests_Review_Assistant.Application.Builders
{
    /// <summary>
    /// Fluent builder interface for constructing a <see cref="ReviewConfiguration"/>.
    /// 
    /// <para>
    ///   Follows the Builder design pattern with Open-Closed Principle:
    ///   <list type="bullet">
    ///   <item>Core review areas are always included.</item>
    ///   <item>Extended areas are added via dedicated methods.</item>
    ///   </list>
    /// </para>
    /// </summary>
    public interface IReviewConfigurationBuilder
    {
        /// <summary>
        /// Sets the target repository platform.
        /// </summary>
        /// 
        /// <param name="platform">The platform type.</param>
        public IReviewConfigurationBuilder ForPlatform(PlatformType platform);

        /// <summary>
        /// Sets the repository coordinates.
        /// </summary>
        /// 
        /// <param name="owner">The repository owner's name.</param>
        /// <param name="name">The repository name.</param>
        public IReviewConfigurationBuilder ForRepository(string owner, string name);  // TODO: rename to "repositoryOwner" and "repositoryName" for clarity

        /// <summary>
        /// Sets the pull request ID.
        /// </summary>
        /// 
        /// <param name="pullRequestId">The pull request ID.</param>
        public IReviewConfigurationBuilder ForPullRequest(int pullRequestId);

        /// <summary>
        /// Enables second-step authorization.
        /// </summary>
        public IReviewConfigurationBuilder WithTwoFactorAuth();

        /// <summary>
        /// Sets the target programming language for standards lookup.
        /// </summary>
        /// 
        /// <param name="language">The programming language.</param>
        public IReviewConfigurationBuilder WithLanguage(string language);

        // -----------------------
        // Additional review areas
        // -----------------------
        
        /// <summary>
        /// Adds code formatting review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeCodeFormatting();

        /// <summary>
        /// Adds linting review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeLinting();

        /// <summary>
        /// Adds copyright header review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeCopyrights();

        /// <summary>
        /// Adds documentation review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeDocumentation();

        /// <summary>
        /// Adds naming convention review.
        /// </summary>
        public  IReviewConfigurationBuilder IncludeNaming();

        /// <summary>
        /// Adds error handling review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeErrorHandling();

        /// <summary>
        /// Adds concurrency review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeConcurrency();

        /// <summary>
        /// Adds testing review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeTesting();

        /// <summary>
        /// Adds dependency management review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeDependencyManagement();

        /// <summary>
        /// Adds accessibility review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeAccessibility();

        /// <summary>
        /// Adds logging review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeLogging();

        /// <summary>
        /// Adds hardcoded secrets detection review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeHardcodedSecrets();

        /// <summary>
        /// Adds dead code review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeDeadCode();

        /// <summary>
        /// Adds complexity review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeComplexity();

        /// <summary>
        /// Adds duplicate code review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeDuplicateCode();

        /// <summary>
        /// Adds API design review.
        /// </summary>
        public IReviewConfigurationBuilder IncludeApiDesign();

        /// <summary>
        /// Adds an arbitrary <see cref="ReviewArea"/> flag (extension point).
        /// </summary>
        /// 
        /// <param name="area">The review area to include.</param>
        public IReviewConfigurationBuilder IncludeArea(ReviewArea area);

        /// <summary>
        /// Includes all available review areas.
        /// </summary>
        public IReviewConfigurationBuilder IncludeAll();

        /// <summary>
        /// Builds the immutable <see cref="ReviewConfiguration"/>.
        /// </summary>
        public ReviewConfiguration Build();
    }
}