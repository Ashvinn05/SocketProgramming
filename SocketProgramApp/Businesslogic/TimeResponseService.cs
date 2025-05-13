using System;

namespace SocketProgramApp.BusinessLogic
{
    /// <summary>
    /// A static class that provides methods to retrieve time response counts based on input.
    /// </summary>
    public static class TimeResponseService
    {
        /// <summary>
        /// Gets the time response count based on the specified input format "set-key".
        /// </summary>
        /// <param name="input">The input string in the format "set-key".</param>
        /// <returns>The time response count if found; otherwise, null.</returns>
        public static int? GetTimeResponseCount(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var parts = input.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;

            string set = parts[0];
            string key = parts[1];

            return ServerDataStore.GetValue(set, key);
        }
    }
}