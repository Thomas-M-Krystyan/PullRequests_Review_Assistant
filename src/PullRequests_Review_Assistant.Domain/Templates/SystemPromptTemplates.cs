using PullRequests_Review_Assistant.Domain.Enums;

namespace PullRequests_Review_Assistant.Domain.Templates
{
    /// <summary>
    /// Central repository for system prompt templates.
    /// Keeps prompt engineering in one place, following DDD value object semantics.
    /// </summary>
    public static class SystemPromptTemplates
    {
        /// <summary>
        /// Core system prompt establishing the agent's persona and review depth.
        /// Written at the level of a Tech Lead / Staff Software Engineer.
        /// </summary>
        public const string CoreReviewPrompt = """
        You are a Senior Code Reviewer acting at the level of a Tech Lead or Staff Software Engineer.
        
        Your responsibilities:
        1. Provide thorough, constructive, and actionable code review feedback.
        2. Focus on correctness, maintainability, readability, and long-term impact.
        3. Cite concrete code lines and suggest specific improvements.
        4. Prioritize issues by severity: critical > warning > info.
        5. Be respectful, educational, and concise.
        
        You MUST output your findings as a JSON array of objects with this schema:
        {
            "filePath": "<relative file path>",
            "line": <line number in the diff>,
            "body": "<markdown comment>",
            "severity": "critical|warning|info",
            "reviewArea": "<area name>"
        }
        
        If no issues are found for a file, return an empty array [].
        """;

        /// <summary>
        /// Builds the review-areas portion of the prompt based on active <see cref="ReviewArea"/> flags.
        /// </summary>
        /// 
        /// <param name="areas">The active review areas.</param>
        /// 
        /// <returns>
        /// A prompt string focusing on the specified review areas.
        /// </returns>
        public static string BuildReviewAreasPrompt(ReviewArea areas)
        {
            var activeAreas = Enum.GetValues<ReviewArea>()
                .Where(area => area is not ReviewArea.None and not ReviewArea.CoreReview and not ReviewArea.All)
                .Where(area => areas.HasFlag(area))
                .Select(area => $"- {area}")
                .ToList();

            return $"""
            
            Focus your review on the following areas:
            {string.Join(Environment.NewLine, activeAreas)}
            
            Ignore areas not listed above.
            """;
        }

        /// <summary>
        /// Template for language-specific standards enrichment.
        /// Ensures consistent formatting regardless of language.
        /// </summary>
        /// 
        /// <param name="language">The programming language name.</param>
        /// <param name="standards">The fetched coding standards text.</param>
        /// 
        /// <returns>
        /// A string template for language-specific standards enrichment.
        /// </returns>
        public static string BuildLanguageStandardsEnrichment(string language, string standards)  // TODO: Rename to "programmingLanguage" and "codingStandards" for clarity
        {
            return $"""
            
            === Language-Specific Coding Standards: {language} ===
            The code under review is written in {language}.
            Apply the following official and community-recommended coding standards:
            
            {standards}
            
            When reviewing, flag deviations from these standards and suggest corrections.
            === End of Language-Specific Standards ===
            """;
        }
    }
}