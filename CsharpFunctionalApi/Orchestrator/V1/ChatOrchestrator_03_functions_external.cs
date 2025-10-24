using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V1.V3;


public static class MessagesFunctions
{
    public static async Task<List<LlmMessage>> BuildMessagesAsync(int sessionId)
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
}


public static class LlmFunctions
{
    public static async Task<LlmResult> GenerateAiResponseAsync(List<LlmMessage> messages)
    {
        // Submit to the LLM
        return await LlmService.GenerateResponseAsync(messages);
    }

    public static async Task<string> ApplyToolCallsAsync(string content)
    {
        // Check for function call
        var finalResult = content;
        var toolCallResult = await LlmService.DoToolCall(finalResult);
        if (toolCallResult.tool == Tool.AddEmoji) finalResult = $"{finalResult} 😊";
        if (toolCallResult.tool == Tool.AddExclamation) finalResult = $"{finalResult} !";
        return finalResult;
    }
}


public static class BusinessFunctions
{
    public static async Task DeductPointsAsync(int playerId, int tokensUsed)
    {
        // Deduct points only after successful response generation
        await EntitlementService.DeductPointsAsync(playerId, tokensUsed);
    }

    public static string BuildFinalResult(string playerName, string finalResult)
    {
        // Build final string to return
        return $"""
            {playerName}!
            {finalResult}

            """;
    }
}
