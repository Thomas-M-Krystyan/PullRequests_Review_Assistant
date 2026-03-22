using NetConsole = System.Console;  // There is a name conflict between System.Console and PullRequests_Review_Assistant.Console namespace

namespace PullRequests_Review_Assistant.Console.Utilities
{
    /// <summary>
    /// Provides generic console prompt helpers for interactive enum selection.
    /// </summary>
    internal static class ConsolePrompt
    {
        /// <summary>
        /// Renders a numbered menu of all values in <typeparamref name="TEnum"/> and loops
        /// until the user enters a valid selection.
        /// </summary>
        ///
        /// <typeparam name="TEnum">The enum type whose values are presented as options.</typeparam>
        ///
        /// <param name="label">
        /// A short display label used in the prompt header, e.g. <c>"platform"</c> or
        /// <c>"subscription tier"</c>. Rendered as: <c>[Config] Select a {label}:</c>
        /// </param>
        ///
        /// <returns>
        /// The <typeparamref name="TEnum"/> value chosen by the user.
        /// </returns>
        internal static TEnum PromptUserSelection<TEnum>(string label)
            where TEnum : struct, Enum
        {
            var values = Enum.GetValues<TEnum>();

            NetConsole.WriteLine($"[Config] Select a {label}:");

            for (var index = 0; index < values.Length; index++)
            {
                var optionNumber = index + 1;
                var optionName = values[index];

                NetConsole.WriteLine($"  [{optionNumber}] {optionName}");
            }

            while (true)
            {
                NetConsole.Write($"Enter a number (1-{values.Length}): ");

                var input = NetConsole.ReadLine()?.Trim();

                if (int.TryParse(input, out var choice) && choice >= 1 && choice <= values.Length)
                {
                    var selected = values[choice - 1];

                    var upperCasedLabel = char.ToUpperInvariant(label[0]) + label[1..];
                    NetConsole.WriteLine($"[Config] {upperCasedLabel} set to '{selected}'.");
                    NetConsole.WriteLine();

                    return selected;
                }

                NetConsole.WriteLine($"[Config] Invalid selection. Please enter a number between 1 and {values.Length}.");
            }
        }
    }
}