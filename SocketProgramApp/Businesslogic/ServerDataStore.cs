using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Represents a static class for managing server data storage.
/// </summary>
namespace SocketProgramApp.BusinessLogic
{
    /// <summary>
    /// A static class for managing server data storage.
    /// </summary>
    public static class ServerDataStore
    {
        /// <summary>
        /// A dictionary that stores sets of key-value pairs.
        /// </summary>
        private static Dictionary<string, Dictionary<string, int>>? _data;

        /// <summary>
        /// Initializes the <see cref="ServerDataStore"/> class by loading data from a JSON file.
        /// </summary>
        static ServerDataStore()
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data.json");
            var json = File.ReadAllText(jsonPath);
            _data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
        }

        /// <summary>
        /// Retrieves the value associated with the specified key from the specified set.
        /// </summary>
        /// <param name="set">The name of the set.</param>
        /// <param name="key">The key whose value is to be retrieved.</param>
        /// <returns>The value associated with the specified key, or null if not found.</returns>
        public static int? GetValue(string set, string key)
        {
            if (_data?.TryGetValue(set, out var subset) == true &&
                subset?.TryGetValue(key, out var value) == true)
                return value;
            return null;
        }
    }
}