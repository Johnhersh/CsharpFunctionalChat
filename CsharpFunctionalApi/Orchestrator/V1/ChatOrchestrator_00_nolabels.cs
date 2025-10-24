using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V1.V0;

public record Session(User Player, int SessionId);
public record User(int Id, string Name);

public static class ChatOrchestrator
{
    public static async Task<string> ProcessMessages(Session session)
    {
        if (!await EntitlementService.HasSufficientPointsAsync(session.Player.Id, 1))
        {
            return "Insufficient points. Please upgrade your subscription to continue chatting.";
        }

        try
        {
            var messages = new List<LlmMessage>();
            var systemPrompt = "Say hello to the user.";
            messages.Add(new LlmMessage("system", systemPrompt));

            var conversationHistory = await DatabaseFunctions.GetSessionMessagesAsync(session.SessionId);
            foreach (var msg in conversationHistory)
            {
                var role = msg.Role == Role.User ? "user" : "assistant";
                messages.Add(new LlmMessage(role, msg.Content));
            }

            var userMessage = new LlmMessage("user", "Hello!");
            messages.Add(userMessage);

            var aiResponseResult = await LlmService.GenerateResponseAsync(messages);
            await EntitlementService.DeductPointsAsync(session.Player.Id, aiResponseResult.TokensUsed);

            var finalResult = aiResponseResult.Response.Content;
            var toolCallResult = await LlmService.DoToolCall(finalResult);
            if (toolCallResult.tool == Tool.AddEmoji) finalResult = $"{finalResult} 😊";
            if (toolCallResult.tool == Tool.AddExclamation) finalResult = $"{finalResult} !";

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
