using System.Text.RegularExpressions;

/// <summary>
/// Provides methods for sanitizing input strings to ensure they meet specific format requirements.
/// </summary>
namespace SocketProgramApp.Utils
{
    /// <summary>
    /// Provides methods for sanitizing input strings to ensure they meet specific format requirements.
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitizes the input string by allowing only alphanumerics and a single dash, and trims whitespace.
        /// </summary>
        /// <param name="input">The input string to sanitize.</param>
        /// <returns>The sanitized string, or null if the input is invalid.</returns>
        public static string? Sanitize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Trim();

            // Only allow A-Z, a-z, 0-9, and a single dash
            if (!Regex.IsMatch(input, @"^[A-Za-z0-9]+-[A-Za-z0-9]+$"))
                return null;

            return input;
        }

        /// <summary>
        /// Validates the format of the input string to ensure it meets specific criteria.
        /// </summary>
        /// <param name="input">The input string to validate.</param>
        /// <returns>True if the input is valid; otherwise, false.</returns>
        public static bool IsValidFormat(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            input = input.Trim();
            return Regex.IsMatch(input, @"^[A-Za-z0-9]+-[A-Za-z0-9]+$");
        }
    }
}
