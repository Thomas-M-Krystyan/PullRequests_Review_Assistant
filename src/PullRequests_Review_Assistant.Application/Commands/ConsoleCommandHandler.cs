using PullRequests_Review_Assistant.Application.Builders;
using PullRequests_Review_Assistant.Application.Services;
using PullRequests_Review_Assistant.Application.Utilities;
using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;
using System.Text;

#pragma warning disable IDE0290  // Disable warnings about using primary constructors

namespace PullRequests_Review_Assistant.Application.Commands
{
    /// <summary>
    /// Interactive console command handler.
    ///
    /// <para>
    /// Parses user input, builds the review configuration via the builder,
    /// and delegates execution to the orchestrator.
    /// </para>
    /// </summary>
    public sealed class ConsoleCommandHandler
    {
        private readonly CodeReviewOrchestrator _orchestrator;
        private readonly ILanguageAgent _languageAgent;
        private readonly ICodeReviewAgent _codeReviewAgent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandHandler"/> class.
        /// </summary>
        /// 
        /// <param name="orchestrator">The orchestrator.</param>
        /// <param name="languageAgent">The language agent.</param>
        /// <param name="codeReviewAgent">The code review agent.</param>
        public ConsoleCommandHandler(
            CodeReviewOrchestrator orchestrator,
            ILanguageAgent languageAgent,
            ICodeReviewAgent codeReviewAgent)
        {
            _orchestrator = orchestrator;
            _languageAgent = languageAgent;
            _codeReviewAgent = codeReviewAgent;
        }

        /// <summary>
        /// Main interactive loop that processes console commands.
        /// </summary>
        /// 
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            PrintHelp();

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("\n> ");
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input is "--help" or "-h" or "--h")
                {
                    PrintHelp();
                    continue;
                }

                if (input is "exit" or "quit")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                if (input.StartsWith("review", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleReviewCommandAsync(input, cancellationToken);
                    continue;
                }

                if (input.StartsWith("language", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleLanguageCommandAsync(input, cancellationToken);
                    continue;
                }

                Console.WriteLine("Unknown command. Type --help for available commands.");
            }
        }

        /// <summary>
        /// Handles the review command asynchronous.
        /// </summary>
        /// 
        /// <param name="input">The input.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// 
        /// <exception cref="ArgumentException"/>
        private async Task HandleReviewCommandAsync(string input, CancellationToken cancellationToken)
        {
            try
            {
                var inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var reviewBuilder = new ReviewConfigurationBuilder();

                // Parse: review <platform> <owner> <repo> <pr-id> [options...]
                if (inputParts.Length < 5)
                {
                    Console.WriteLine("Usage: review <github|gitlab|bitbucket> <owner> <repo> <pr-id> [options]");
                    Console.WriteLine("Options: --2fa --lang=<language> --formatting --linting --copyrights");
                    Console.WriteLine("         --docs --naming --errors --concurrency --testing --deps");
                    Console.WriteLine("         --a11y --logging --secrets --deadcode --complexity --duplicates");
                    Console.WriteLine("         --api --all");

                    return;
                }

                var platform = inputParts[1];
                var owner = inputParts[2];
                var name = inputParts[3];
                var pullRequestId = inputParts[4];

                // Use ConsolePrompt.ParseArg for platform parsing
                var platformType = ConsolePrompt.ParseArg<PlatformType>([$"--platform={platform}"], "platform")
                                   ?? throw new ArgumentException($"Invalid platform: {platform}");

                reviewBuilder.ForPlatform(platformType)
                    .ForRepository(owner: owner, name: name)
                    .ForPullRequest(int.Parse(pullRequestId));

                // Parse optional flags
                for (var index = 5; index < inputParts.Length; index++)
                {
                    var reviewOption = inputParts[index].ToLowerInvariant();
                    _ = reviewOption switch
                    {
                        "--2fa"         => reviewBuilder.WithTwoFactorAuth(),
                        "--formatting"  => reviewBuilder.IncludeCodeFormatting(),
                        "--linting"     => reviewBuilder.IncludeLinting(),
                        "--copyrights"  => reviewBuilder.IncludeCopyrights(),
                        "--docs"        => reviewBuilder.IncludeDocumentation(),
                        "--naming"      => reviewBuilder.IncludeNaming(),
                        "--errors"      => reviewBuilder.IncludeErrorHandling(),
                        "--concurrency" => reviewBuilder.IncludeConcurrency(),
                        "--testing"     => reviewBuilder.IncludeTesting(),
                        "--deps"        => reviewBuilder.IncludeDependencyManagement(),
                        "--a11y"        => reviewBuilder.IncludeAccessibility(),
                        "--logging"     => reviewBuilder.IncludeLogging(),
                        "--secrets"     => reviewBuilder.IncludeHardcodedSecrets(),
                        "--deadcode"    => reviewBuilder.IncludeDeadCode(),
                        "--complexity"  => reviewBuilder.IncludeComplexity(),
                        "--duplicates"  => reviewBuilder.IncludeDuplicateCode(),
                        "--api"         => reviewBuilder.IncludeApiDesign(),
                        "--all"         => reviewBuilder.IncludeAll(),

                        // Language option
                        _ when reviewOption.StartsWith("--lang=") => reviewBuilder.WithLanguage(reviewOption["--lang=".Length..]),

                        // Unrecognized option
                        _ => throw new ArgumentException($"Unknown option: {reviewOption}")
                    };
                }

                var reviewConfiguration = reviewBuilder.Build();

                Console.WriteLine($"[Review] Starting review for {reviewConfiguration.Platform} " +
                                  $"{reviewConfiguration.RepositoryOwner}/{reviewConfiguration.RepositoryName} PR #{reviewConfiguration.PullRequestId}");
                Console.WriteLine($"[Review] Areas: {reviewConfiguration.Areas}");

                var comments = await _orchestrator.RunReviewAsync(reviewConfiguration, cancellationToken);
                Console.WriteLine($"[Review] Completed with {comments.Count} comment(s).");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[Error] {exception.Message}");
            }
        }

        /// <summary>
        /// Handles the language command asynchronous.
        /// </summary>
        /// 
        /// <param name="input">The input.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task HandleLanguageCommandAsync(string input, CancellationToken cancellationToken)
        {
            try
            {
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    Console.WriteLine("Usage: language <C#|Python|Java|Go|Rust|JavaScript|TypeScript|C|C++>");
                    return;
                }

                var language = string.Join(' ', parts[1..]);
                Console.WriteLine($"[Language] Fetching standards for {language}...");

                var enrichment = await _languageAgent.GetLanguageStandardsPromptAsync(language, cancellationToken);
                _codeReviewAgent.UpdateSystemPrompt(enrichment);

                Console.WriteLine($"[Language] Code review agent updated with {language} standards.");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[Error] {exception.Message}");
            }
        }

        /// <summary>
        /// Prints the help menu with available commands and options.
        /// </summary>
        private static void PrintHelp()
        {
            Console.OutputEncoding = Encoding.UTF8;  // Ensure proper rendering of box-drawing characters

            Console.WriteLine("""
                              ╔══════════════════════════════════════════════════════════════════╗
                              ║              PR Review Assistant — Commands                      ║
                              ╠══════════════════════════════════════════════════════════════════╣
                              ║                                                                  ║
                              ║  review <platform> <owner> <repo> <pr-id> [options]              ║
                              ║    Platforms: github, gitlab, bitbucket                          ║
                              ║                                                                  ║
                              ║    Core areas (always included):                                 ║
                              ║      Performance, Architecture, Vulnerabilities, CodeSmells      ║
                              ║                                                                  ║
                              ║    Options (extended review areas):                              ║
                              ║      --2fa          Enable two-factor auth                       ║
                              ║      --lang=<lang>  Set target language (C#, Python, etc.)       ║
                              ║      --formatting   Include code formatting review               ║
                              ║      --linting      Include linting review                       ║
                              ║      --copyrights   Include copyright header review              ║
                              ║      --docs         Include documentation review                 ║
                              ║      --naming       Include naming convention review             ║
                              ║      --errors       Include error handling review                ║
                              ║      --concurrency  Include concurrency review                   ║
                              ║      --testing      Include testing review                       ║
                              ║      --deps         Include dependency management review         ║
                              ║      --a11y         Include accessibility review                 ║
                              ║      --logging      Include logging review                       ║
                              ║      --secrets      Include hardcoded secrets review             ║
                              ║      --deadcode     Include dead code review                     ║
                              ║      --complexity   Include complexity review                    ║
                              ║      --duplicates   Include duplicate code review                ║
                              ║      --api          Include API design review                    ║
                              ║      --all          Include ALL review areas                     ║
                              ║                                                                  ║
                              ║  language <lang>    Set/change review language at runtime        ║
                              ║  --help / -h        Show this help                               ║
                              ║  exit / quit        Exit the application                         ║
                              ╚══════════════════════════════════════════════════════════════════╝
                              """);
        }
    }
}