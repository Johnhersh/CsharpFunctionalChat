using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V1.V2;

public record Session(User Player, int SessionId);
public record User(int Id, string Name);

public static class ChatOrchestrator
{
    public static async Task<string> ProcessMessages(Session session)
    {
        // Check if user has sufficient points
        if (!await EntitlementService.HasSufficientPointsAsync(session.Player.Id, 1))
        {
            return "Insufficient points. Please upgrade your subscription to continue chatting.";
        }

        try
        {
            var messages = await BuildMessagesAsync(session.SessionId);
            var aiResponseResult = await GenerateAiResponseAsync(messages);
            await DeductPointsAsync(session.Player.Id, aiResponseResult.TokensUsed);
            var finalResult = await ApplyToolCallsAsync(aiResponseResult.Response.Content);
            return BuildFinalResult(session.Player.Name, finalResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "An error occurred while processing your request. Please try again later.";
        }
    }

    private static async Task<List<LlmMessage>> BuildMessagesAsync(int sessionId)
    {
        var messages = new List<LlmMessage>();

        // Add system message first
        var systemPrompt = "Say hello to the user.";
        messages.Add(new LlmMessage("system", systemPrompt));

        // Generate AI response FIRST using existing conversation history and current user message
        var conversationHistory = await DatabaseFunctions.GetSessionMessagesAsync(sessionId);
        foreach (var msg in conversationHistory)
        {
            var role = msg.Role == Role.User ? "user" : "assistant";
            messages.Add(new LlmMessage(role, msg.Content));
        }

        // Add current user message
        var userMessage = new LlmMessage("user", "Hello!");
        messages.Add(userMessage);

        return messages;
    }

    private static async Task<LlmResult> GenerateAiResponseAsync(List<LlmMessage> messages)
    {
        // Submit to the LLM
        return await LlmService.GenerateResponseAsync(messages);
    }

    private static async Task DeductPointsAsync(int playerId, int tokensUsed)
    {
        // Deduct points only after successful response generation
        await EntitlementService.DeductPointsAsync(playerId, tokensUsed);
    }

    private static async Task<string> ApplyToolCallsAsync(string content)
    {
        // Check for function call
        var finalResult = content;
        var toolCallResult = await LlmService.DoToolCall(finalResult);
        if (toolCallResult.tool == Tool.AddEmoji) finalResult = $"{finalResult} 😊";
        if (toolCallResult.tool == Tool.AddExclamation) finalResult = $"{finalResult} !";
        return finalResult;
    }

    private static string BuildFinalResult(string playerName, string finalResult)
    {
        // Build final string to return
        return $"""
            {playerName}!
            {finalResult}

            """;
    }
}
