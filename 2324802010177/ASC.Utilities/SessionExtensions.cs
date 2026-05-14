using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ASC.Utilities
{
    public static class SessionExtensions
    {
        public static void SetSession<T>(this ISession session, string key, T value)
            => session.SetString(key, JsonConvert.SerializeObject(value));

        public static T? GetSession<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }

    public class CurrentUser
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsEngineer { get; set; }
        public bool IsUser { get; set; }
    }
}
