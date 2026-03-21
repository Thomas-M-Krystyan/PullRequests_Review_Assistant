using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PullRequests_Review_Assistant.Infrastructure.Configuration
{
    /// <summary>
    /// Provides LLM model configuration from a JSON configuration file.
    /// Supports per-subscription-tier model preferences with fallback.
    /// </summary>
    public sealed class ModelConfigProvider : IModelConfigProvider
    {
        #region Constants
        private const string DefaultConfigPath = "model-config.json";
        private const string DefaultFallbackModel = "gpt-4o";
        #endregion

        #region JSON Serialization Options
        private static readonly JsonSerializerOptions _readSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
        };

        private readonly JsonSerializerOptions _writeSerializerOptions = new(_readSerializerOptions)
        {
            WriteIndented = true
        };
        #endregion

        private readonly Dictionary<SubscriptionTier, ModelPreference> _preferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConfigProvider"/> class.
        /// </summary>
        /// 
        /// <param name="configPath">The configuration path.</param>
        public ModelConfigProvider(string? configPath = null)
        {
            var path = configPath ?? DefaultConfigPath;
            // Read configuration from the "model-config.json" file if it exists
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var root = JsonSerializer.Deserialize<ModelConfigRoot>(json, _readSerializerOptions);

                _preferences = root?.GitHubCopilotPlans ?? BuildDefaults();
            }
            // Write default configuration if the "model-config.json" file doesn't exist
            else
            {
                _preferences = BuildDefaults();

                // Wrap defaults under the "GitHub_Copilot_Plans" node and write for user convenience
                var root = new ModelConfigRoot { GitHubCopilotPlans = _preferences };
                var json = JsonSerializer.Serialize(root, _writeSerializerOptions);
                File.WriteAllText(path, json);

                Console.WriteLine($"[Config] Default model configuration written to {path}");
            }
        }

        /// <inheritdoc />
        public ModelPreference GetPreferredModel(SubscriptionTier tier)
        {
            return _preferences.TryGetValue(tier, out var pref)
                ? pref
                : new ModelPreference
                {
                    PrimaryModel = DefaultFallbackModel,
                    SecondaryModel = DefaultFallbackModel,
                    FallbackModel = DefaultFallbackModel
                };
        }

        /// <inheritdoc />
        public async Task<string> ResolveModelAsync(SubscriptionTier tier, CancellationToken cancellationToken = default)
        {
            var preference = GetPreferredModel(tier);

            // Set the model via environment variable for the Copilot CLI
            Environment.SetEnvironmentVariable("GITHUB_COPILOT_MODEL", preference.PrimaryModel);

            try
            {
                // Validate availability by a lightweight check
                // In production, this could ping the model endpoint
                await Task.CompletedTask;
                Console.WriteLine($"[Model] Using primary model: {preference.PrimaryModel}");
                return preference.PrimaryModel;
            }
            catch
            {
                Console.WriteLine($"[Model] Primary model '{preference.PrimaryModel}' unavailable. " +
                                  $"Falling back to '{preference.FallbackModel}'.");
                Environment.SetEnvironmentVariable("GITHUB_COPILOT_MODEL", preference.FallbackModel);
                return preference.FallbackModel;
            }
        }

        /// <summary>
        /// Builds the default model preferences for each subscription tier.
        /// </summary>
        /// 
        /// <returns>
        /// A dictionary mapping each <see cref="SubscriptionTier"/> to its default <see cref="ModelPreference"/>.
        /// </returns>
        private static Dictionary<SubscriptionTier, ModelPreference> BuildDefaults() => new()
        {
            [SubscriptionTier.Free] = new ModelPreference
            {
                PrimaryModel = "gpt-4.1",
                SecondaryModel = "claude-haiku-4.5",
                FallbackModel = DefaultFallbackModel
            },
            [SubscriptionTier.Student] = new ModelPreference
            {
                PrimaryModel = "gemini-3.1-pro",
                SecondaryModel = "gpt-4.1",
                FallbackModel = DefaultFallbackModel
            },
            [SubscriptionTier.Pro] = new ModelPreference
            {
                PrimaryModel = "claude-opus-4.6",
                SecondaryModel = "claude-sonnet-4.6",
                FallbackModel = DefaultFallbackModel
            },
            [SubscriptionTier.ProPlus] = new ModelPreference
            {
                PrimaryModel = "claude-opus-4.6",
                SecondaryModel = "claude-sonnet-4.6",
                FallbackModel = DefaultFallbackModel
            },
            [SubscriptionTier.Business] = new ModelPreference
            {
                PrimaryModel = "claude-opus-4.6",
                SecondaryModel = "claude-sonnet-4.6",
                FallbackModel = DefaultFallbackModel
            },
            [SubscriptionTier.Enterprise] = new ModelPreference
            {
                PrimaryModel = "claude-opus-4.6",
                SecondaryModel = "claude-sonnet-4.6",
                FallbackModel = DefaultFallbackModel
            }
        };

        /// <summary>
        /// Intermediate wrapper matching the top-level "GitHub_Copilot_Plans" node in <c>model-config.json</c>.
        /// </summary>
        private sealed class ModelConfigRoot
        {
            [JsonPropertyName("GitHub_Copilot_Plans")]
            public Dictionary<SubscriptionTier, ModelPreference>? GitHubCopilotPlans { get; init; }
        }
    }
}