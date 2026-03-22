using PullRequests_Review_Assistant.Application.Builders;
using PullRequests_Review_Assistant.Application.Commands;
using PullRequests_Review_Assistant.Application.Services;
using PullRequests_Review_Assistant.Application.Utilities;
using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;
using PullRequests_Review_Assistant.Infrastructure.Agents;
using PullRequests_Review_Assistant.Infrastructure.Auth.Factory;
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
        /// <summary>
        /// The entry point of the application. Sets up configuration, authentication,
        /// platform services, agents, and the console command handler, then starts the
        /// interactive loop.
        /// 
        /// </summary>
        /// <param name="args">The arguments.</param>
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
                var tier = ConsolePrompt.ParseArg<SubscriptionTier>(args, "tier") ?? ConsolePrompt.PromptUserSelection<SubscriptionTier>("Tier");
                var resolvedModel = await modelConfig.ResolveModelAsync(tier, cts.Token);

                // Secrets + Auth
                var secrets = new UserSecretsSecretsProvider("pr-review-assistant-local");
                var authFactory = new AuthStrategyFactory(secrets);

                // Platform (user selects; the command handler picks per-review)
                var platformType = ConsolePrompt.ParseArg<PlatformType>(args, "platform") ?? ConsolePrompt.PromptUserSelection<PlatformType>("Platform");
                var authStrategy = authFactory.Create(platformType);
                var platform = CreatePlatformService(platformType, authStrategy);
                await platform.InitializeAsync(cancellationToken: cts.Token);

                // Agents
                var codeReviewAgent = new CopilotCodeReviewAgent(resolvedModel);
                var languageAgent = new CopilotLanguageStandardsAgent();

                // Orchestrator
                var orchestrator = new CodeReviewOrchestrator(
                    codeReviewAgent, languageAgent, platform);

                // Review builder
                var reviewBuilder = new ReviewConfigurationBuilder();

                // Console UI
                var commandHandler = new ConsoleCommandHandler(
                    orchestrator, languageAgent, codeReviewAgent, reviewBuilder);

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