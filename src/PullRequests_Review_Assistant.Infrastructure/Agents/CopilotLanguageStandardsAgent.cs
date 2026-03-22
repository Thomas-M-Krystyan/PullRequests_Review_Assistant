using GitHub.Copilot.SDK;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Domain.Templates;

namespace PullRequests_Review_Assistant.Infrastructure.Agents
{
    /// <summary>
    /// Language agent that uses GitHub Copilot SDK to fetch
    /// the most recent and recommended coding standards for a given language,
    /// then formats them as a system prompt enrichment for the code review agent.
    /// </summary>
    public sealed class CopilotLanguageStandardsAgent : ILanguageStandardsAgent, IAsyncDisposable
    {
        private readonly CopilotClient _copilotClient;

        private const string SystemPrompt = """
                                            You are a programming language standards expert.
                                            When given a programming language name, you must:
                                            1. Identify the most recent official style guide and coding standards.
                                            2. Identify the most widely recommended community coding standards.
                                            3. Summarize the key rules, conventions, and best practices.
                                            4. Be specific: include naming conventions, formatting rules, error handling patterns,
                                               idiomatic patterns, and any language-specific pitfalls.
                                            5. Respond ONLY with the standards summary — no preamble, no opinions.
                                            """;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopilotLanguageStandardsAgent"/> class.
        /// </summary>
        public CopilotLanguageStandardsAgent()
        {
            _copilotClient = new CopilotClient();
        }

        /// <inheritdoc />
        public async Task<string> GetLanguageStandardsPromptAsync(
            string programmingLanguage, CancellationToken cancellationToken = default)
        {
            var prompt = $"""
                          What are the most recent and recommended official coding standards 
                          and best practices for {programmingLanguage}? 
                          Include the official style guide if one exists, popular community standards, 
                          naming conventions, formatting rules, idiomatic patterns, 
                          and common pitfalls to avoid.
                          """;

            await using var session = await _copilotClient.CreateSessionAsync(new SessionConfig
            {
                SystemMessage = new SystemMessageConfig
                {
                    Content = SystemPrompt
                }
            },
            cancellationToken);

            var response = await session.SendAndWaitAsync(new MessageOptions
            {
                Prompt = prompt
            },
            cancellationToken: cancellationToken);

            return SystemPromptTemplates.BuildLanguageStandardsEnrichment(
                programmingLanguage, response?.Data.Content ?? string.Empty);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _copilotClient.DisposeAsync();
        }
    }
}