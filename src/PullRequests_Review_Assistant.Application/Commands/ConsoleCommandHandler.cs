using PullRequests_Review_Assistant.Application.Services;
using PullRequests_Review_Assistant.Application.Utilities;
using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.Interfaces;
using System.Text;
using PullRequests_Review_Assistant.Application.Builders.Interface;

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
        private readonly ILanguageStandardsAgent _languageStandardsAgent;
        private readonly ICodeReviewAgent _codeReviewAgent;
        private readonly IReviewConfigurationBuilder _reviewBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandHandler"/> class.
        /// </summary>
        /// 
        /// <param name="orchestrator">The orchestrator.</param>
        /// <param name="languageStandardsAgent">The language agent.</param>
        /// <param name="codeReviewAgent">The code review agent.</param>
        /// <param name="reviewBuilder">The review configuration builder.</param>
        public ConsoleCommandHandler(
            CodeReviewOrchestrator orchestrator,
            ILanguageStandardsAgent languageStandardsAgent,
            ICodeReviewAgent codeReviewAgent,
            IReviewConfigurationBuilder reviewBuilder)
        {
            _orchestrator = orchestrator;
            _languageStandardsAgent = languageStandardsAgent;
            _codeReviewAgent = codeReviewAgent;
            _reviewBuilder = reviewBuilder;
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

                // Parse: review <platform> <repositoryOwner> <repo> <pr-id> [options...]
                if (inputParts.Length < 5)
                {
                    Console.WriteLine("Usage: review <github|gitlab|bitbucket> <repositoryOwner> <repo> <pr-id> [options]");
                    Console.WriteLine("Options: --2fa --lang=<language> --formatting --linting --copyrights");
                    Console.WriteLine("         --docs --naming --errors --concurrency --testing --deps");
                    Console.WriteLine("         --a11y --logging --secrets --deadcode --complexity --duplicates");
                    Console.WriteLine("         --api --area=<ReviewArea> --all");

                    return;
                }

                var platform = inputParts[1];
                var owner = inputParts[2];
                var name = inputParts[3];
                var pullRequestId = inputParts[4];

                // Use ConsolePrompt.ParseArg for platform parsing
                var platformType = ConsolePrompt.ParseArg<PlatformType>([$"--platform={platform}"], "platform")
                                   ?? throw new ArgumentException($"Invalid platform: {platform}");

                _reviewBuilder.ForPlatform(platformType)
                    .ForRepository(repositoryOwner: owner, name: name)
                    .ForPullRequest(int.Parse(pullRequestId));

                // Parse optional flags
                for (var index = 5; index < inputParts.Length; index++)
                {
                    var reviewOption = inputParts[index].ToLowerInvariant();
                    _ = reviewOption switch
                    {
                        "--2fa"         => _reviewBuilder.WithTwoFactorAuth(),
                        "--formatting"  => _reviewBuilder.IncludeCodeFormatting(),
                        "--linting"     => _reviewBuilder.IncludeLinting(),
                        "--copyrights"  => _reviewBuilder.IncludeCopyrights(),
                        "--docs"        => _reviewBuilder.IncludeDocumentation(),
                        "--naming"      => _reviewBuilder.IncludeNaming(),
                        "--errors"      => _reviewBuilder.IncludeErrorHandling(),
                        "--concurrency" => _reviewBuilder.IncludeConcurrency(),
                        "--testing"     => _reviewBuilder.IncludeTesting(),
                        "--deps"        => _reviewBuilder.IncludeDependencyManagement(),
                        "--a11y"        => _reviewBuilder.IncludeAccessibility(),
                        "--logging"     => _reviewBuilder.IncludeLogging(),
                        "--secrets"     => _reviewBuilder.IncludeHardcodedSecrets(),
                        "--deadcode"    => _reviewBuilder.IncludeDeadCode(),
                        "--complexity"  => _reviewBuilder.IncludeComplexity(),
                        "--duplicates"  => _reviewBuilder.IncludeDuplicateCode(),
                        "--api"         => _reviewBuilder.IncludeApiDesign(),
                        "--all"         => _reviewBuilder.IncludeAll(),

                        // Language option
                        _ when reviewOption.StartsWith("--lang=") => _reviewBuilder.WithLanguage(reviewOption["--lang=".Length..]),

                        // ReviewArea option
                        _ when reviewOption.StartsWith("--area=") => Enum.TryParse<ReviewArea>(inputParts[index]["--area=".Length..], ignoreCase: true, out var area)
                            ? _reviewBuilder.IncludeArea(area)
                            : throw new ArgumentException($"Unknown review area: '{inputParts[index]["--area=".Length..]}'.  Valid values: {string.Join(", ", Enum.GetNames<ReviewArea>())}"),

                        // Unrecognized option
                        _ => throw new ArgumentException($"Unknown option: {reviewOption}")
                    };
                }

                var reviewConfiguration = _reviewBuilder.Build();

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

                var enrichment = await _languageStandardsAgent.GetLanguageStandardsPromptAsync(language, cancellationToken);
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
                              ║  review <platform> <repositoryOwner> <repo> <pr-id> [options]              ║
                              ║    Platforms: github, gitlab, bitbucket                          ║
                              ║                                                                  ║
                              ║    Core areas (always included):                                 ║
                              ║      Performance, Architecture, Vulnerabilities, CodeSmells      ║
                              ║                                                                  ║
                              ║    Options (extended review areas):                              ║
                              ║      --2fa               Enable two-factor auth                  ║
                              ║      --lang=<lang>       Set target language (C#, Python, etc.)  ║
                              ║      --formatting        Include code formatting review          ║
                              ║      --linting           Include linting review                  ║
                              ║      --copyrights        Include copyright header review         ║
                              ║      --docs              Include documentation review            ║
                              ║      --naming            Include naming convention review        ║
                              ║      --errors            Include error handling review           ║
                              ║      --concurrency       Include concurrency review              ║
                              ║      --testing           Include testing review                  ║
                              ║      --deps              Include dependency management review    ║
                              ║      --a11y              Include accessibility review            ║
                              ║      --logging           Include logging review                  ║
                              ║      --secrets           Include hardcoded secrets review        ║
                              ║      --deadcode          Include dead code review                ║
                              ║      --complexity        Include complexity review               ║
                              ║      --duplicates        Include duplicate code review           ║
                              ║      --api               Include API design review               ║
                              ║      --area=<ReviewArea> Include an arbitrary review area        ║
                              ║      --all               Include ALL review areas                ║
                              ║                                                                  ║
                              ║  language <lang>    Set/change review language at runtime        ║
                              ║  --help / -h        Show this help                               ║
                              ║  exit / quit        Exit the application                         ║
                              ╚══════════════════════════════════════════════════════════════════╝
                              """);
        }
    }
}