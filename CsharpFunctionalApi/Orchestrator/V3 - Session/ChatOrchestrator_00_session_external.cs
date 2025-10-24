using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V3;


public static class MessagesFunctions
{
    public static async Task<CoreContext> BuildMessagesAsync(CoreContext context)
    {
        var messages = new List<LlmMessage>();

        // Add system message first
        var systemPrompt = "Say hello to the user.";
        messages.Add(new LlmMessage("system", systemPrompt));

        // Generate AI response FIRST using existing conversation history and current user message
        var conversationHistory = await DatabaseFunctions.GetSessionMessagesAsync(context.Session.SessionId);
        foreach (var msg in conversationHistory)
        {
            var role = msg.Role == Role.User ? "user" : "assistant";
            messages.Add(new LlmMessage(role, msg.Content));
        }

        // Add current user message
        var userMessage = new LlmMessage("user", "Hello!");
        context.Messages.Add(userMessage);

        return context;
    }
}


public static class LlmFunctions
{
    public static async Task<CoreContext> GenerateAiResponseAsync(CoreContext context)
    {
        // Submit to the LLM
        context.AiResponseResult = await LlmService.GenerateResponseAsync(context.Messages);
        return context;
    }

    public static async Task ApplyToolCallsAsync(CoreContext context)
    {
        // Check for function call
        var finalResult = context.AiResponseResult.Response.Content;
        var toolCallResult = await LlmService.DoToolCall(finalResult);
        if (toolCallResult.tool == Tool.AddEmoji) finalResult = $"{finalResult} 😊";
        if (toolCallResult.tool == Tool.AddExclamation) finalResult = $"{finalResult} !";
        context.FinalResult = finalResult;
    }
}


public static class BusinessFunctions
{
    public static async Task DeductPointsAsync(CoreContext context)
    {
        // Deduct points only after successful response generation
        await EntitlementService.DeductPointsAsync(context.Session.Player.Id, context.AiResponseResult.TokensUsed);
    }

    public static string BuildFinalResult(CoreContext context)
    {
        // Build final string to return
        return $"""
            {context.Session.Player.Name}!
            {context.FinalResult}

            """;
    }
}
