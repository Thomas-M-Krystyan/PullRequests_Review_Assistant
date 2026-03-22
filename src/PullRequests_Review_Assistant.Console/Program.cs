using PullRequests_Review_Assistant.Application.Commands;
using PullRequests_Review_Assistant.Application.Services;
using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Infrastructure.Agents;
using PullRequests_Review_Assistant.Infrastructure.Auth;
using PullRequests_Review_Assistant.Infrastructure.Configuration;
using PullRequests_Review_Assistant.Infrastructure.Platform;
using PullRequests_Review_Assistant.Infrastructure.Secrets;
using System.Text;
using NetConsole = System.Console;  // There is a name conflict between System.Console and PullRequests_Review_Assistant.Console namespace

namespace PullRequests_Review_Assistant.Console
{
    /// <summary>
    /// Application entry point and composition root.
    /// Wires up all layers (Domain => Application => Infrastructure)
    /// and starts the interactive console loop.
    /// </summary>
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            NetConsole.OutputEncoding = Encoding.UTF8;  // Ensure proper rendering of box-drawing characters

            NetConsole.WriteLine("╔══════════════════════════════════════════════════╗");
            NetConsole.WriteLine("║     PR Review Assistant — Powered by Copilot     ║");
            NetConsole.WriteLine("╚══════════════════════════════════════════════════╝");
            NetConsole.WriteLine();

            using var cts = new CancellationTokenSource();

            NetConsole.CancelKeyPress += HandleCancelKeyPress;

            try
            {
                // Configuration
                var modelConfig = new ModelConfigProvider();
                var tier = ParseSubscriptionTier(args) ?? PromptTierType();
                var resolvedModel = await modelConfig.ResolveModelAsync(tier, cts.Token);

                // Secrets + Auth
                var secrets = new AzureKeyVaultSecretsProvider();
                var authFactory = new AuthStrategyFactory(secrets);

                // Platform (user selects; the command handler picks per-review)
                var platformType = ParsePlatformType(args) ?? PromptPlatformType();
                var authStrategy = authFactory.Create(platformType);
                var platform = CreatePlatformService(platformType, authStrategy);
                await platform.InitializeAsync(cancellationToken: cts.Token);

                // Agents
                var codeReviewAgent = new CopilotCodeReviewAgent(resolvedModel);
                await codeReviewAgent.InitializeAsync(cts.Token);

                var languageAgent = new CopilotLanguageAgent();
                await languageAgent.InitializeAsync(cts.Token);

                // Orchestrator
                var orchestrator = new CodeReviewOrchestrator(
                    codeReviewAgent, languageAgent, platform);

                // Console UI
                var commandHandler = new ConsoleCommandHandler(
                    orchestrator, languageAgent, codeReviewAgent);

                await commandHandler.RunAsync(cts.Token);

                // Cleanup
                await codeReviewAgent.DisposeAsync();
                await languageAgent.DisposeAsync();
                await platform.DisposeAsync();
            }
            finally
            {
                NetConsole.CancelKeyPress -= HandleCancelKeyPress;
            }

            return;

            // Local function to handle Ctrl+C gracefully
            void HandleCancelKeyPress(object? sender, ConsoleCancelEventArgs consoleCancelEventArgs)
            {
                consoleCancelEventArgs.Cancel = true;

                // ReSharper disable AccessToDisposedClosure
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
                // ReSharper restore AccessToDisposedClosure
            }
        }

        /// <summary>
        /// Parses the subscription tier.
        /// </summary>
        /// 
        /// <param name="args">The arguments.</param>
        /// 
        /// <returns>
        /// The parsed <see cref="SubscriptionTier"/>, or <see cref="SubscriptionTier.Free"/> if not specified.
        /// </returns>
        private static SubscriptionTier? ParseSubscriptionTier(string[] args)
        {
            var tierArg = args.FirstOrDefault(line => line.StartsWith("--tier=", StringComparison.OrdinalIgnoreCase));
            if (tierArg is null)
            {
                NetConsole.WriteLine($"[Config] No --tier specified in '{args}'.");

                return null;
            }

            var value = tierArg["--tier=".Length..];
            if (Enum.TryParse<SubscriptionTier>(value, ignoreCase: true, out var tier))
            {
                return tier;
            }

            NetConsole.WriteLine($"[Config] Unrecognised tier '{value}'.");

            return null;
        }
        
        /// <summary>
        /// Prompts the user to choose a tier interactively from the list of supported options.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="SubscriptionTier"/> selected by the user.
        /// </returns>
        private static SubscriptionTier PromptTierType()
        {
            var tiers = Enum.GetValues<SubscriptionTier>();

            NetConsole.WriteLine("[Config] Select a subscription tier:");

            for (var index = 0; index < tiers.Length; index++)
            {
                var tierNumber = index + 1;
                var tierName = tiers[index];

                NetConsole.WriteLine($"  [{tierNumber}] {tierName}");
            }

            while (true)
            {
                NetConsole.Write($"Enter a number (1–{tiers.Length}): ");

                var input = NetConsole.ReadLine()?.Trim();

                if (int.TryParse(input, out var choice) && choice >= 1 && choice <= tiers.Length)
                {
                    var selected = tiers[choice - 1];
                    NetConsole.WriteLine($"[Config] Tier set to '{selected}'.");
                    NetConsole.WriteLine();

                    return selected;
                }

                NetConsole.WriteLine($"[Config] Invalid selection. Please enter a number between 1 and {tiers.Length}.");
            }
        }

        /// <summary>
        /// Attempts to resolve the platform type from a <c>--platform=</c> command-line argument.
        /// </summary>
        /// 
        /// <param name="args">The arguments.</param>
        /// 
        /// <returns>
        /// The parsed <see cref="PlatformType"/>, or <see langword="null"/> if the argument is absent or invalid.
        /// </returns>
        private static PlatformType? ParsePlatformType(string[] args)
        {
            var platformArg = args.FirstOrDefault(line => line.StartsWith("--platform=", StringComparison.OrdinalIgnoreCase));
            if (platformArg is null)
            {
                NetConsole.WriteLine($"[Config] No --platform specified in '{args}'.");

                return null;
            }

            var value = platformArg["--platform=".Length..];
            if (Enum.TryParse<PlatformType>(value, ignoreCase: true, out var platform))
            {
                return platform;
            }

            NetConsole.WriteLine($"[Config] Unrecognised platform '{value}'.");

            return null;
        }

        /// <summary>
        /// Prompts the user to choose a platform interactively from the list of supported options.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="PlatformType"/> selected by the user.
        /// </returns>
        private static PlatformType PromptPlatformType()
        {
            var platforms = Enum.GetValues<PlatformType>();

            NetConsole.WriteLine("[Config] Select a platform:");

            for (var index = 0; index < platforms.Length; index++)
            {
                var platformNumber = index + 1;
                var platformName = platforms[index];

                NetConsole.WriteLine($"  [{platformNumber}] {platformName}");
            }

            while (true)
            {
                NetConsole.Write($"Enter a number (1–{platforms.Length}): ");

                var input = NetConsole.ReadLine()?.Trim();

                if (int.TryParse(input, out var choice) && choice >= 1 && choice <= platforms.Length)
                {
                    var selected = platforms[choice - 1];
                    NetConsole.WriteLine($"[Config] Platform set to '{selected}'.");
                    NetConsole.WriteLine();

                    return selected;
                }

                NetConsole.WriteLine($"[Config] Invalid selection. Please enter a number between 1 and {platforms.Length}.");
            }
        }

        /// <summary>
        /// Instantiates the correct <see cref="IRepositoryPlatformService"/> implementation
        /// for the given <paramref name="platformType"/>.
        /// </summary>
        /// 
        /// <param name="platformType">The resolved platform.</param>
        /// <param name="authStrategy">The authentication strategy for that platform.</param>
        /// 
        /// <returns>
        /// An uninitialised <see cref="IRepositoryPlatformService"/> ready for <c>InitializeAsync</c>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentException"/>
        private static IRepositoryPlatformService CreatePlatformService(
            PlatformType platformType, IAuthStrategy authStrategy) => platformType switch
        {
            PlatformType.GitHub    => new GitHubPlatformService(authStrategy),
            PlatformType.GitLab    => new GitLabPlatformService(authStrategy),
            PlatformType.Bitbucket => new BitbucketPlatformService(authStrategy),

            _ => throw new ArgumentException($"Unsupported platform: {platformType}", nameof(platformType))
        };
    }
}