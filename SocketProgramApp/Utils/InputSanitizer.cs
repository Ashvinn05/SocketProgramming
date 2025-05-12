using System.Text.RegularExpressions;

namespace SocketProgramApp.Utils
{
    public static class InputSanitizer
    {
        // Allows only alphanumerics and a single dash, trims whitespace
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

        public static bool IsValidFormat(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            input = input.Trim();
            return Regex.IsMatch(input, @"^[A-Za-z0-9]+-[A-Za-z0-9]+$");
        }
    }
}
