using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V2.V0;

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
            // The good: Now the variables don't pollute this space. The data is contained in one place.
            // The bad: More verbose, and unrelated data is coupled in the context object. Session is still scoped here.
            // some values are null because they don't exist until later steps
            var context = await MessagesFunctions.BuildMessagesAsync(session.SessionId, new CoreContext());
            context = await LlmFunctions.GenerateAiResponseAsync(context);
            await BusinessFunctions.DeductPointsAsync(session.Player.Id, context);
            await LlmFunctions.ApplyToolCallsAsync(context);
            return BusinessFunctions.BuildFinalResult(session.Player.Name, context);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "An error occurred while processing your request. Please try again later.";
        }
    }
}
