namespace Api.Common;

public static class ValidationHelper
{
    public static bool IsValidChatRoomName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && 
               name.Length <= AppConstants.Validation.MaxChatRoomNameLength;
    }

    public static bool IsValidPassword(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= 3;
    }

    public static bool IsValidUsername(string username)
    {
        return !string.IsNullOrWhiteSpace(username) && 
               username.Length >= 2 && 
               username.Length <= 50;
    }

    public static bool IsValidMessageContent(string content)
    {
        return !string.IsNullOrWhiteSpace(content) && content.Length <= 1000;
    }
}
