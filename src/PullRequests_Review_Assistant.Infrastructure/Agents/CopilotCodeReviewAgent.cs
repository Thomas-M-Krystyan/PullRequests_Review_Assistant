using GitHub.Copilot.SDK;
using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Domain.Templates;
using PullRequests_Review_Assistant.Domain.ValueObjects;
using System.Text.Json;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Infrastructure.Agents
{
    /// <summary>
    /// Code review agent powered by GitHub Copilot SDK.
    /// Uses <see cref="CopilotClient"/> to perform AI-driven code reviews.
    /// </summary>
    public sealed class CopilotCodeReviewAgent : ICodeReviewAgent, IAsyncDisposable
    {
        private readonly string _modelId;
        private readonly CopilotClient _copilotClient;
        private string _additionalPrompt = string.Empty;

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CopilotCodeReviewAgent"/> class.
        /// </summary>
        /// 
        /// <param name="modelId">The model identifier.</param>
        public CopilotCodeReviewAgent(string modelId)
        {
            _modelId = modelId;
            _copilotClient = new CopilotClient();
        }

        /// <inheritdoc />
        public void UpdateSystemPrompt(string additionalPrompt)
        {
            _additionalPrompt = additionalPrompt;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ReviewComment>> ReviewFileAsync(
            PullRequestFile file, ReviewConfiguration config, CancellationToken cancellationToken = default)
        {
            var systemPrompt = BuildFullSystemPrompt(config.Areas) + _additionalPrompt;

            var userPrompt = $"""
                              Review the following file diff from a pull request.

                              File: {file.FilePath}
                              Language: {file.Language}

                              Diff:
                              ```
                              {file.DiffContent}
                              ```

                              {$"Full file content:\n```\n{file.FullContent}\n```"}

                              Produce your review comments as a JSON array.
                              """;

            await using var session = await _copilotClient.CreateSessionAsync(new SessionConfig
            {
                Model = _modelId,
                SystemMessage = new SystemMessageConfig
                {
                    Content = systemPrompt
                }
            },
            cancellationToken);

            var response = await session.SendAndWaitAsync(new MessageOptions
            {
                Prompt = userPrompt
            },
            cancellationToken: cancellationToken);

            return ParseReviewComments(response?.Data.Content ?? string.Empty, file.FilePath);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _copilotClient.DisposeAsync();
        }

        /// <summary>
        /// Builds the full system prompt for the agent.
        /// </summary>
        /// 
        /// <param name="areas">The code review areas.</param>
        /// 
        /// <returns>
        /// The full system prompt for the agent.
        /// </returns>
        private static string BuildFullSystemPrompt(ReviewArea areas)
        {
            return SystemPromptTemplates.CoreReviewPrompt
                   + SystemPromptTemplates.BuildReviewAreasPrompt(areas);
        }

        /// <summary>
        /// Parses the agent's response into a list of <see cref="ReviewComment"/> objects.
        /// </summary>
        /// 
        /// <param name="response">The agent's response.</param>
        /// <param name="fallbackFilePath">The fallback file path for error reporting.</param>
        /// 
        /// <returns>
        /// A list of review comments.
        /// </returns>
        private static List<ReviewComment> ParseReviewComments(string response, string fallbackFilePath)
        {
            try
            {
                // Extract JSON array from the response (agent may include markdown fences)
                var jsonStart = response.IndexOf('[');
                var jsonEnd = response.LastIndexOf(']');

                if (jsonStart < 0 || jsonEnd < 0 || jsonEnd <= jsonStart)
                {
                    return [];
                }

                var json = response[jsonStart..(jsonEnd + 1)];

                var comments = JsonSerializer.Deserialize<List<ReviewComment>>(json, _serializerOptions);

                return comments ?? [];
            }
            catch (JsonException)
            {
                Console.WriteLine($"[Warning] Could not parse review comments for {fallbackFilePath}. Skipping.");

                return [];
            }
        }
    }
}