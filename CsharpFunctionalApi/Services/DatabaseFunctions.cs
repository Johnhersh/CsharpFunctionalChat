namespace CsharpFunctionalApi.Services;

public enum Role
{
    User,
    Character,
}
public record CharacterMessage(Role Role, string Content);

public static class DatabaseFunctions
{
    public static async Task<List<CharacterMessage>> GetSessionMessagesAsync(int sessionId)
    {
        // Simulate an asynchronous database call
        await Task.Delay(50);
        // For demonstration, return a static list of messages
        List<CharacterMessage> result =
        [
            new CharacterMessage(Role.User, "Hello!"),
            new CharacterMessage(Role.Character, "Hi there! How can I assist you today?")
        ];

        return result;
    }
}
