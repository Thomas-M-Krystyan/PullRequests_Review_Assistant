using PullRequests_Review_Assistant.Domain.Enums;

namespace PullRequests_Review_Assistant.Domain.ValueObjects
{
    /// <summary>
    /// Immutable configuration snapshot produced by the builder.
    ///
    /// <para>
    /// Captures every parameter needed to execute a code review session.
    /// </para>
    /// </summary>
    public sealed record ReviewConfiguration
    {
        /// <summary>
        /// Bitwise combination of <see cref="ReviewArea"/> flags.
        /// </summary>
        public required ReviewArea Areas { get; init; }

        /// <summary>
        /// Target programming language (e.g., "C#", "Python", "TypeScript").
        /// </summary>
        public string TargetLanguage { get; init; } = string.Empty;

        /// <summary>
        /// Language-specific coding standards appended to the system prompt.
        /// </summary>
        public string LanguageStandards { get; init; } = string.Empty;

        /// <summary>
        /// The hosting platform for the pull request.
        /// </summary>
        public required PlatformType Platform { get; init; }

        /// <summary>
        /// Repository owner or namespace.
        /// </summary>
        public required string RepositoryOwner { get; init; }

        /// <summary>
        /// Repository name.
        /// </summary>
        public required string RepositoryName { get; init; }

        /// <summary>
        /// Pull Request (PR) / Merge Request (MR) identifier.
        /// </summary>
        public required int PullRequestId { get; init; }

        /// <summary>
        /// Whether two-factor / second-step authorization is required.
        /// </summary>
        public bool RequiresTwoFactorAuth { get; init; }
    }
}