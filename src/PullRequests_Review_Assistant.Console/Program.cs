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
using PullRequests_Review_Assistant.Console.Utilities;
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
                var tier = ParseSubscriptionTier(args) ?? ConsolePrompt.PromptUserSelection<SubscriptionTier>("Tier");
                var resolvedModel = await modelConfig.ResolveModelAsync(tier, cts.Token);

                // Secrets + Auth
                var secrets = new AzureKeyVaultSecretsProvider();
                var authFactory = new AuthStrategyFactory(secrets);

                // Platform (user selects; the command handler picks per-review)
                var platformType = ParsePlatformType(args) ?? ConsolePrompt.PromptUserSelection<PlatformType>("Platform");
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