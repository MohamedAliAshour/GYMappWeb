using System.Text.Json;

namespace GYMappWeb.Models
{
    public static class SessionHelper
    {
        private const string UserSessionKey = "UserSession";

        public class UserSession
        {
            public string Id { get; set; }
            public string UserName { get; set; }
        }

        public static void SetUserSession(this ISession session, string id, string userName)
        {
            var userSession = new UserSession
            {
                Id = id,
                UserName = userName
            };
            string serializedSession = JsonSerializer.Serialize(userSession);
            session.SetString(UserSessionKey, serializedSession);
        }

        public static UserSession GetUserSession(this ISession session)
        {
            string serializedSession = session.GetString(UserSessionKey);
            if (string.IsNullOrEmpty(serializedSession))
            {
                return null;
            }
            return JsonSerializer.Deserialize<UserSession>(serializedSession);
        }

        public static void ClearUserSession(this ISession session)
        {
            session.Remove(UserSessionKey);
        }
    }
}
