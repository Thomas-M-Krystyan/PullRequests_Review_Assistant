namespace PullRequests_Review_Assistant.Domain.Interfaces
{
    /// <summary>
    /// Agent responsible for fetching official coding standards for a language
    /// and producing a system prompt enrichment for the code review agent.
    /// </summary>
    public interface ILanguageStandardsAgent
    {
        /// <summary>
        /// Fetches recommended coding standards for <paramref name="programmingLanguage"/>
        /// and returns a formatted system prompt enrichment string.
        /// </summary>
        /// 
        /// <param name="programmingLanguage">The programming language to fetch standards for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// 
        /// <returns>A formatted system prompt enrichment string.</returns>
        public Task<string> GetLanguageStandardsPromptAsync(string programmingLanguage, CancellationToken cancellationToken = default);
    }
}