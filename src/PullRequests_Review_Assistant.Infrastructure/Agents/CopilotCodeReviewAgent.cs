using GitHub.Copilot.SDK;
using Microsoft.Agents.AI;
using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Domain.Templates;
using PullRequests_Review_Assistant.Domain.ValueObjects;
using System.Text.Json;

namespace PullRequests_Review_Assistant.Infrastructure.Agents
{
    /// <summary>
    /// Code review agent powered by GitHub Copilot SDK.
    /// Uses <see cref="CopilotClient"/> and the Microsoft Agent Framework
    /// to perform AI-driven code reviews.
    /// </summary>
    public sealed class CopilotCodeReviewAgent : ICodeReviewAgent, IAsyncDisposable
    {
        private readonly string _modelId;
        private readonly CopilotClient _copilotClient;
        private AIAgent? _agent;
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

            // The GitHub Copilot SDK resolves the model via the GITHUB_COPILOT_MODEL
            // environment variable. Pin it here so every agent (re)creation uses the
            // correct model, regardless of any later environment changes
            Environment.SetEnvironmentVariable("GITHUB_COPILOT_MODEL", _modelId);

            _copilotClient = new CopilotClient();
        }

        /// <summary>
        /// Starts the Copilot client and creates the AI agent.
        /// Must be called before any review operations.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await _copilotClient.StartAsync(cancellationToken);

            _agent = CreateAgent(BuildFullSystemPrompt(ReviewArea.CoreReview));
        }

        /// <inheritdoc />
        public void UpdateSystemPrompt(string additionalPrompt)
        {
            _additionalPrompt = additionalPrompt;

            // Recreate agent with updated instructions
            _agent = CreateAgent(BuildFullSystemPrompt(ReviewArea.CoreReview) + _additionalPrompt);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException"/>
        public async Task<IReadOnlyList<ReviewComment>> ReviewFileAsync(
            PullRequestFile file, ReviewConfiguration config, CancellationToken cancellationToken = default)
        {
            if (_agent is null)
            {
                throw new InvalidOperationException("Agent not initialized. Call InitializeAsync first.");
            }

            var systemPrompt = BuildFullSystemPrompt(config.Areas);
            var fullPrompt = systemPrompt + _additionalPrompt;

            // Reconstruct agent with current review areas
            _agent = CreateAgent(fullPrompt);

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

            var response = await _agent.RunAsync(userPrompt, cancellationToken: cancellationToken);

            return ParseReviewComments(response.Text, file.FilePath);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _copilotClient.DisposeAsync();
        }

        /// <summary>
        /// Creates an <see cref="AIAgent"/> pinned to <see cref="_modelId"/> with the given instructions.
        /// </summary>
        /// 
        /// <remarks>
        /// The GitHub Copilot SDK selects the model via the <c>GITHUB_COPILOT_MODEL</c>
        /// environment variable. The variable is set here before every agent creation to
        /// guarantee the correct model is used even if the environment has changed since
        /// construction.
        /// </remarks>
        /// 
        /// <param name="instructions">The system prompt instructions for the agent.</param>
        /// 
        /// <returns>
        /// A new <see cref="AIAgent"/> instance.
        /// </returns>
        private AIAgent CreateAgent(string instructions)
        {
            Environment.SetEnvironmentVariable("GITHUB_COPILOT_MODEL", _modelId);

            return _copilotClient.AsAIAgent(instructions: instructions);
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