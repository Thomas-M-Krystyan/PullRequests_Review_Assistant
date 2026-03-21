namespace PullRequests_Review_Assistant.Domain.Entities
{
    /// <summary>
    /// Represents a single file changed in a pull request, including its diff content.
    /// </summary>
    public sealed class PullRequestFile
    {
        /// <summary>
        /// Relative path of the file in the repository.
        /// </summary>
        public required string FilePath { get; init; }

        /// <summary>
        /// Unified diff content for the file.
        /// </summary>
        public required string DiffContent { get; init; }

        /// <summary>
        /// Full content of the file at the PR head commit.
        /// </summary>
        public string FullContent { get; init; } = string.Empty;

        /// <summary>
        /// Programming language inferred from the file extension.
        /// </summary>
        public string Language { get; init; } = string.Empty;
    }
}