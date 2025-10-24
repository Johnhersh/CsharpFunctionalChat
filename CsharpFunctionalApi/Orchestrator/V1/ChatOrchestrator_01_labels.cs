using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V1.V1;

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
            var messages = new List<LlmMessage>();

            // Add system message first
            var systemPrompt = "Say hello to the user.";
            messages.Add(new LlmMessage("system", systemPrompt));

            // Generate AI response FIRST using existing conversation history and current user message
            var conversationHistory = await DatabaseFunctions.GetSessionMessagesAsync(session.SessionId);
            foreach (var msg in conversationHistory)
            {
                var role = msg.Role == Role.User ? "user" : "assistant";
                messages.Add(new LlmMessage(role, msg.Content));
            }

            // Add current user message
            var userMessage = new LlmMessage("user", "Hello!");
            messages.Add(userMessage);

            // Submit to the LLM
            var aiResponseResult = await LlmService.GenerateResponseAsync(messages);

            // Deduct points only after successful response generation
            await EntitlementService.DeductPointsAsync(session.Player.Id, aiResponseResult.TokensUsed);

            // Check for function call
            var finalResult = aiResponseResult.Response.Content;
            var toolCallResult = await LlmService.DoToolCall(finalResult);
            if (toolCallResult.tool == Tool.AddEmoji) finalResult = $"{finalResult} 😊";
            if (toolCallResult.tool == Tool.AddExclamation) finalResult = $"{finalResult} !";

            // Build final string to return
            var result =
                $"""
            {session.Player.Name}!
            {finalResult}
            
            """;


            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "An error occurred while processing your request. Please try again later.";
        }

    }
}
