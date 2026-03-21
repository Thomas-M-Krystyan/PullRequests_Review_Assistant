using GitHub.Copilot.SDK;
using Microsoft.Agents.AI;
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
    /// Uses <see cref="CopilotClient"/> and the Microsoft Agent Framework
    /// to perform AI-driven code reviews.
    /// </summary>
    public sealed class CopilotCodeReviewAgent : ICodeReviewAgent, IAsyncDisposable
    {
        private readonly CopilotClient _copilotClient;
        private AIAgent? _agent;
        private string _additionalPrompt = string.Empty;
        private readonly string _modelId;  // TODO: Use this for model selection when creating the agent

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

        /// <summary>
        /// Starts the Copilot client and creates the AI agent.
        /// Must be called before any review operations.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await _copilotClient.StartAsync(cancellationToken);

            _agent = _copilotClient.AsAIAgent(
                instructions: BuildFullSystemPrompt(ReviewArea.CoreReview));
        }

        /// <inheritdoc />
        public void UpdateSystemPrompt(string additionalPrompt)
        {
            _additionalPrompt = additionalPrompt;

            // Recreate agent with updated instructions
            _agent = _copilotClient.AsAIAgent(
                instructions: BuildFullSystemPrompt(ReviewArea.CoreReview) + _additionalPrompt);
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
            _agent = _copilotClient.AsAIAgent(instructions: fullPrompt);

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

        private static string BuildFullSystemPrompt(ReviewArea areas)
        {
            return SystemPromptTemplates.CoreReviewPrompt
                   + SystemPromptTemplates.BuildReviewAreasPrompt(areas);
        }

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