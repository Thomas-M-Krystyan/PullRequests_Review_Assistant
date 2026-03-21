namespace PullRequests_Review_Assistant.Infrastructure.Extensions
{
    /// <summary>
    /// String extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Infers programming language from file extension (extracted from file path) for syntax highlighting in review comments.
        /// </summary>
        ///
        /// <param name="filePath">The file path.</param>
        ///
        /// <returns>
        /// A string representing the programming language, or empty if unknown.
        /// </returns>
        public static string InferLanguage(this string? filePath)
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