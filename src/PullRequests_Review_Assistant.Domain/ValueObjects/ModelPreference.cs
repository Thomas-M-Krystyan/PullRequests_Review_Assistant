namespace PullRequests_Review_Assistant.Domain.ValueObjects
{
    /// <summary>
    /// Represents a preferred LLM model specialized in code review tasks,
    /// with secondary option (to compare results) and a fallback option.
    /// 
    /// <para>
    ///   Immutable value object used in model configuration.
    /// </para>
    /// </summary>
    public sealed record ModelPreference
    {
        /// <summary>
        /// The primary model identifier (e.g., "claude-opus-4.6", "gemini-3.1-pro").
        /// </summary>
        public required string PrimaryModel { get; init; }

        /// <summary>
        /// The secondary model identifier (e.g., "claude-sonnet-4.6", "gpt-4.1").
        /// </summary>
        public required string SecondaryModel { get; init; }  // TODO: Think what to do with secondary model (select desired model from UI or use both and compare results?)

        /// <summary>
        /// The fallback model identifier (e.g., "gpt-4o") used when the primary or secondary model is unavailable.
        /// </summary>
        public required string FallbackModel { get; init; }
    }
}