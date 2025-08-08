namespace Api.Common;

public static class AppConstants
{
    public static class Roles
    {
        public const string User = "User";
        public const string Admin = "Admin";
    }

    public static class Policies
    {
        public const string AllowClients = "AllowClients";
    }

    public static class ClaimTypes
    {
        public const string UserId = System.Security.Claims.ClaimTypes.NameIdentifier;
        public const string Username = System.Security.Claims.ClaimTypes.Name;
        public const string Identity = "unique_name";
    }

    public static class SignalR
    {
        public const string ChatHubEndpoint = "/chathub";
        public const string UserJoinedEvent = "UserJoined";
        public const string UserLeftEvent = "UserLeft";
        public const string ReceiveMessageEvent = "ReceiveMessage";
    }

    public static class Encryption
    {
        public const int AesKeySize = 32;
        public const int AesIvSize = 16;
    }

    public static class Validation
    {
        public const int MaxChatRoomNameLength = 100;
        public const int MaxRetryAttempts = 10;
    }
}
