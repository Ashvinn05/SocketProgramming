using System;

namespace SocketProgramApp.BusinessLogic
{
    public static class TimeResponseService
    {
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