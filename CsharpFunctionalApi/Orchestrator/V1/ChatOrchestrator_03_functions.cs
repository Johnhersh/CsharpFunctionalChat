using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V1.V3;

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
            var messages = await MessagesFunctions.BuildMessagesAsync(session.SessionId);
            var aiResponseResult = await LlmFunctions.GenerateAiResponseAsync(messages);
            await BusinessFunctions.DeductPointsAsync(session.Player.Id, aiResponseResult.TokensUsed);
            var finalResult = await LlmFunctions.ApplyToolCallsAsync(aiResponseResult.Response.Content);
            return BusinessFunctions.BuildFinalResult(session.Player.Name, finalResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "An error occurred while processing your request. Please try again later.";
        }
    }
}
