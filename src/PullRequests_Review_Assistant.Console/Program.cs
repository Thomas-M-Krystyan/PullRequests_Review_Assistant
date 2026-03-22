using PullRequests_Review_Assistant.Application.Commands;
using PullRequests_Review_Assistant.Application.Services;
using PullRequests_Review_Assistant.Domain.Enums;
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

            ConsoleCancelEventHandler handler = (_, consoleCancelEventArgs) =>
            {
                consoleCancelEventArgs.Cancel = true;

                // ReSharper disable AccessToDisposedClosure
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
                // ReSharper restore AccessToDisposedClosure
            };

            NetConsole.CancelKeyPress += handler;

            try
            {
                // Configuration
                var modelConfig = new ModelConfigProvider();
                var tier = ParseSubscriptionTier(args);
                var resolvedModel = await modelConfig.ResolveModelAsync(tier, cts.Token);

                // Secrets + Auth
                var secrets = new AzureKeyVaultSecretsProvider();
                var authFactory = new AuthStrategyFactory(secrets);

                // Platform (default to GitHub; the command handler picks per-review)
                var githubAuthStrategy = authFactory.Create(PlatformType.GitHub);
                var githubPlatform = new GitHubPlatformService(githubAuthStrategy);
                await githubPlatform.InitializeAsync(cancellationToken: cts.Token);

                // Agents
                var codeReviewAgent = new CopilotCodeReviewAgent(resolvedModel);
                await codeReviewAgent.InitializeAsync(cts.Token);

                var languageAgent = new CopilotLanguageAgent();
                await languageAgent.InitializeAsync(cts.Token);

                // Orchestrator
                var orchestrator = new CodeReviewOrchestrator(
                    codeReviewAgent, languageAgent, githubPlatform);

                // Console UI
                var commandHandler = new ConsoleCommandHandler(
                    orchestrator, languageAgent, codeReviewAgent);

                await commandHandler.RunAsync(cts.Token);

                // Cleanup
                await codeReviewAgent.DisposeAsync();
                await languageAgent.DisposeAsync();
                await githubPlatform.DisposeAsync();
            }
            finally
            {
                NetConsole.CancelKeyPress -= handler;
            }
        }

        private static SubscriptionTier ParseSubscriptionTier(string[] args)
        {
            var tierArg = args.FirstOrDefault(a => a.StartsWith("--tier=", StringComparison.OrdinalIgnoreCase));

            if (tierArg is not null)
            {
                var value = tierArg["--tier=".Length..];
                if (Enum.TryParse<SubscriptionTier>(value, ignoreCase: true, out var tier))
                {
                    return tier;
                }
            }

            NetConsole.WriteLine("[Config] No --tier specified. Defaulting to 'Free'.");

            return SubscriptionTier.Free;
        }
    }
}