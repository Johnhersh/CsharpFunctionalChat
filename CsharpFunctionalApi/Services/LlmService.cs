namespace CsharpFunctionalApi.Services;

public record LlmResult(LlmMessage Response, int TokensUsed);
public record LlmMessage(string Role, string Content);

public enum Tool
{
    AddEmoji,
    AddExclamation,
    ToUpperCase
}
public record ToolCallResult(Tool tool);

public static class LlmService
{
    private readonly static LlmMessage[] possibleResults =
    [
        new LlmMessage("assistant", "Hello! How can I help you?"),
        new LlmMessage("assistant", "Hi there! What would you like to know?"),
        new LlmMessage("assistant", "Greetings! How may I assist you today?")
    ];

    public static async Task<LlmResult> GenerateResponseAsync(List<LlmMessage> messages)
    {
        // Simulate an asynchronous call to an LLM service
        await Task.Delay(100);
        // For demonstration, return a static response
        var randomResultIndex = new Random().Next(0, possibleResults.Length);
        var randomResult = possibleResults[randomResultIndex];
        return new LlmResult(randomResult, 5);
    }

    public static async Task<ToolCallResult> DoToolCall(string llmMessage)
    {
        // Pretend to query an LLM for tool call
        await Task.Delay(100);

        return new ToolCallResult(Tool.AddEmoji);
    }
}
