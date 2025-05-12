using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SocketProgramApp.BusinessLogic
{
    public static class ServerDataStore
    {
        private static Dictionary<string, Dictionary<string, int>>? _data;

        static ServerDataStore()
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data.json");
            var json = File.ReadAllText(jsonPath);
            _data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
        }

        public static int? GetValue(string set, string key)
        {
            if (_data?.TryGetValue(set, out var subset) == true &&
                subset?.TryGetValue(key, out var value) == true)
                return value;
            return null;
        }
    }
}