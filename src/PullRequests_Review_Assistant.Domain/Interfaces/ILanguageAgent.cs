namespace PullRequests_Review_Assistant.Domain.Interfaces
{
    /// <summary>
    /// Agent responsible for fetching official coding standards for a language
    /// and producing a system prompt enrichment for the code review agent.
    /// </summary>
    public interface ILanguageAgent  // TODO: Rename to ICodeStandardsAgent
    {
        /// <summary>
        /// Fetches recommended coding standards for <paramref name="language"/>
        /// and returns a formatted system prompt enrichment string.
        /// </summary>
        /// 
        /// <param name="language">The programming language to fetch standards for.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// 
        /// <returns>A formatted system prompt enrichment string.</returns>
        public Task<string> GetLanguageStandardsPromptAsync(string language, CancellationToken cancellationToken = default);  // TODO: Rename parameter to "programmingLanguage"
    }
}