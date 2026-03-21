namespace PullRequests_Review_Assistant.Domain.Entities
{
    /// <summary>
    /// A review comment to be posted on a specific line of a pull request file.
    /// </summary>
    public sealed class ReviewComment
    {
        /// <summary>
        /// Relative file path the comment applies to.
        /// </summary>
        public required string FilePath { get; init; }

        /// <summary>
        /// Line number in the diff where the comment should appear.
        /// </summary>
        public required int Line { get; init; }

        /// <summary>
        /// The review comment body (Markdown).
        /// </summary>
        public required string Body { get; init; }

        /// <summary>
        /// Severity: info, warning, or critical.
        /// </summary>
        public required string Severity { get; init; }

        /// <summary>
        /// Which <see cref="Domain.Enums.ReviewArea"/> this comment relates to.
        /// </summary>
        public required string ReviewArea { get; init; }
    }
}