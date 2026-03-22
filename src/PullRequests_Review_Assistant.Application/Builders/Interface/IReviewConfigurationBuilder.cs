using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.ValueObjects;

namespace PullRequests_Review_Assistant.Application.Builders.Interface
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
        IReviewConfigurationBuilder ForPlatform(PlatformType platform);

        /// <summary>
        /// Sets the repository coordinates.
        /// </summary>
        /// 
        /// <param name="repositoryOwner">The repository owner's name.</param>
        /// <param name="repositoryName">The repository name.</param>
        IReviewConfigurationBuilder ForRepository(string repositoryOwner, string repositoryName);

        /// <summary>
        /// Sets the pull request ID.
        /// </summary>
        /// 
        /// <param name="pullRequestId">The pull request ID.</param>
        IReviewConfigurationBuilder ForPullRequest(int pullRequestId);

        /// <summary>
        /// Enables second-step authorization.
        /// </summary>
        IReviewConfigurationBuilder WithTwoFactorAuth();

        /// <summary>
        /// Sets the target programming language for standards lookup.
        /// </summary>
        /// 
        /// <param name="language">The programming language.</param>
        IReviewConfigurationBuilder WithLanguage(string language);

        // -----------------------
        // Additional review areas
        // -----------------------

        /// <summary>
        /// Adds code formatting review.
        /// </summary>
        IReviewConfigurationBuilder IncludeCodeFormatting();

        /// <summary>
        /// Adds linting review.
        /// </summary>
        IReviewConfigurationBuilder IncludeLinting();

        /// <summary>
        /// Adds copyright header review.
        /// </summary>
        IReviewConfigurationBuilder IncludeCopyrights();

        /// <summary>
        /// Adds documentation review.
        /// </summary>
        IReviewConfigurationBuilder IncludeDocumentation();

        /// <summary>
        /// Adds naming convention review.
        /// </summary>
        IReviewConfigurationBuilder IncludeNaming();

        /// <summary>
        /// Adds error handling review.
        /// </summary>
        IReviewConfigurationBuilder IncludeErrorHandling();

        /// <summary>
        /// Adds concurrency review.
        /// </summary>
        IReviewConfigurationBuilder IncludeConcurrency();

        /// <summary>
        /// Adds testing review.
        /// </summary>
        IReviewConfigurationBuilder IncludeTesting();

        /// <summary>
        /// Adds dependency management review.
        /// </summary>
        IReviewConfigurationBuilder IncludeDependencyManagement();

        /// <summary>
        /// Adds accessibility review.
        /// </summary>
        IReviewConfigurationBuilder IncludeAccessibility();

        /// <summary>
        /// Adds logging review.
        /// </summary>
        IReviewConfigurationBuilder IncludeLogging();

        /// <summary>
        /// Adds hardcoded secrets detection review.
        /// </summary>
        IReviewConfigurationBuilder IncludeHardcodedSecrets();

        /// <summary>
        /// Adds dead code review.
        /// </summary>
        IReviewConfigurationBuilder IncludeDeadCode();

        /// <summary>
        /// Adds complexity review.
        /// </summary>
        IReviewConfigurationBuilder IncludeComplexity();

        /// <summary>
        /// Adds duplicate code review.
        /// </summary>
        IReviewConfigurationBuilder IncludeDuplicateCode();

        /// <summary>
        /// Adds API design review.
        /// </summary>
        IReviewConfigurationBuilder IncludeApiDesign();

        /// <summary>
        /// Adds an arbitrary <see cref="ReviewArea"/> flag (extension point).
        /// </summary>
        /// 
        /// <param name="area">The review area to include.</param>
        IReviewConfigurationBuilder IncludeArea(ReviewArea area);

        /// <summary>
        /// Includes all available review areas.
        /// </summary>
        IReviewConfigurationBuilder IncludeAll();

        /// <summary>
        /// Builds the immutable <see cref="ReviewConfiguration"/>.
        /// </summary>
        ReviewConfiguration Build();
    }
}