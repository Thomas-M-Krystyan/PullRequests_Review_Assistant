using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using PullRequests_Review_Assistant.Domain.Entities;
using PullRequests_Review_Assistant.Domain.Interfaces;

namespace PullRequests_Review_Assistant.Infrastructure.Platform
{
    /// <summary>
    /// GitHub platform service using the MCP GitHub server
    /// to fetch PR files and post review comments.
    /// </summary>
    public sealed class GitHubPlatformService : IRepositoryPlatformService, IAsyncDisposable
    {
        private IMcpClient? _mcpClient;

        /// <summary>
        /// Initializes the MCP client connected to the GitHub MCP server.
        /// Requires GITHUB_PERSONAL_ACCESS_TOKEN environment variable.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _mcpClient = await McpClientFactory.CreateAsync(
                // The transport layer defines how the client communicates with the server
                new StdioClientTransport(new StdioClientTransportOptions
                {
                    Name = "GitHubMCP",
                    Command = "npx",
                    Arguments = ["-y", "@modelcontextprotocol/server-github"],
                }),
                // Optionally, client info can be specified for better logging and debugging on the server side
                new McpClientOptions
                {
                    ClientInfo = new Implementation { Name = "GitHubMCP", Version = "1.0.0" }
                },
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(
            string owner, string repo, int pullRequestId, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var result = await _mcpClient!.CallToolAsync("get_pull_request_files", new Dictionary<string, object?>
            {
                ["owner"] = owner,
                ["repo"] = repo,
                ["pull_number"] = pullRequestId
            }, cancellationToken: cancellationToken);

            List<PullRequestFile> files = [];

            files.AddRange(
                result.Content.OfType<TextContentBlock>()
                    .Select(content => new PullRequestFile
                    {
                        FilePath = content.Text,
                        DiffContent = content.Text,
                        Language = InferLanguage(content.Text)
                    }));

            return files;
        }

        /// <inheritdoc />
        public async Task PostReviewCommentAsync(
            string owner, string repo, int pullRequestId,
            ReviewComment comment, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            await _mcpClient!.CallToolAsync("create_pull_request_review", new Dictionary<string, object?>
            {
                ["owner"] = owner,
                ["repo"] = repo,
                ["pull_number"] = pullRequestId,
                ["body"] = $"[{comment.Severity.ToUpperInvariant()}] ({comment.ReviewArea}) {comment.Body}",
                ["event"] = "COMMENT",
                ["comments"] = new[]
                {
                    new
                    {
                        path = comment.FilePath,
                        line = comment.Line,
                        body = $"**[{comment.Severity}]** _{comment.ReviewArea}_\n\n{comment.Body}"
                    }
                }
            }, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_mcpClient is not null)
            {
                await _mcpClient.DisposeAsync();
            }
        }

        /// <summary>
        /// Ensures the MCP client is initialized before making API calls.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        private void EnsureInitialized()
        {
            if (_mcpClient is null)
            {
                throw new InvalidOperationException("MCP client not initialized. Call InitializeAsync first.");
            }
        }

        /// <summary>
        /// Infers programming language from file extension for syntax highlighting in review comments.
        /// </summary>
        ///
        /// <param name="filePath">The file path.</param>
        ///
        /// <returns>
        /// A string representing the programming language, or empty if unknown.
        /// </returns>
        private static string InferLanguage(string? filePath)
        {
            if (filePath is null)
            {
                return string.Empty;
            }

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            return fileExtension switch
            {
                // Programming languages
                ".cs"       => "C#",
                ".vb"       => "VB.NET",
                ".fs"       => "F#",
                ".java"     => "Java",
                ".kt"       => "Kotlin",
                ".scala"    => "Scala",
                ".go"       => "Go",
                ".rs"       => "Rust",
                ".c"        => "C",
                ".h"        => "C Header",
                ".cpp" or ".cc" or ".cxx" or ".hpp" => "C++",
                ".m"        => "Objective-C",
                ".mm"       => "Objective-C++",
                ".swift"    => "Swift",
                ".dart"     => "Dart",
                ".php"      => "PHP",
                ".rb"       => "Ruby",
                ".r"        => "R",
                ".jl"       => "Julia",

                // Scripting languages
                ".js"       => "JavaScript",
                ".jsx"      => "JavaScript (React)",
                ".ts"       => "TypeScript",
                ".tsx"      => "TypeScript (React)",
                ".py"       => "Python",
                ".lua"      => "Lua",
                ".sh"       => "Shell Script",
                ".bash"     => "Bash",
                ".zsh"      => "Z Shell",
                ".ps1"      => "PowerShell",
                ".bat"      => "Batch Script",
                ".cmd"      => "Windows Command Script",

                // Markups and stylesheets
                ".html" or ".htm" => "HTML",
                ".css"      => "CSS",
                ".scss"     => "SCSS",
                ".sass"     => "SASS",
                ".less"     => "LESS",
                ".xml"      => "XML",
                ".xaml"     => "XAML",
                ".md"       => "Markdown",
                ".adoc"     => "AsciiDoc",
                ".mustache" => "Mustache",
                ".hbs"      => "Handlebars",

                // Query languages
                ".sql"      => "SQL",
                ".graphql" or ".gql"  => "GraphQL",

                // Data formats
                ".json"     => "JSON",
                ".jsonc"    => "JSON with Comments",
                ".yaml" or ".yml" => "YAML",
                ".toml"     => "TOML",
                ".ini"      => "INI",

                // DevOps / IaC
                ".tf"       => "Terraform",
                ".tfvars"   => "Terraform Variables",
                ".dockerfile" or "dockerfile" => "Dockerfile",
                ".env"      => "Environment Variables",
                ".cfg"      => "Config File",

                // Build systems
                ".gradle"   => "Gradle",
                ".groovy"   => "Groovy",
                ".cmake"    => "CMake",
                ".make" or "makefile" => "Makefile",

                // Version control
                ".gitignore" => "Git Ignore",
                ".gitconfig" => "Git Config",
                ".gitattributes" => "Git Attributes",

                // Containerization
                ".dockerignore" => "Docker Ignore",

                _ => string.Empty
            };
        }
    }
}