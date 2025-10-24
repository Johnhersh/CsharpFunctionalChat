using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V3;

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
            // We introduce Session into CoreContext and make it required. That way we can guarantee it exists.
            // Now we don't need to pass session data separately.
            // The good: Now we don't see any variables here except the context!
            // The bad: Some stuff is still nullable in CoreContext
            var context = new CoreContext { Session = session };

            context = await MessagesFunctions.BuildMessagesAsync(context);
            context = await LlmFunctions.GenerateAiResponseAsync(context);
            await BusinessFunctions.DeductPointsAsync(context);
            await LlmFunctions.ApplyToolCallsAsync(context);
            return BusinessFunctions.BuildFinalResult(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "An error occurred while processing your request. Please try again later.";
        }
    }
}
