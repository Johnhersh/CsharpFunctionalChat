using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V4;

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
            // The good: Now we don't see any variables here!
            var pipeline = new PipelineData(new CoreContext { Session = session });

            var pipelineWithMessages = await pipeline.BuildMessagesAsync();
            var pipelineWithAiResponse = await pipelineWithMessages.GenerateAiResponseAsync();
            // Note: At this point the pipeline doesn't have any functions! It's not possible to call GenerateAiResponseAsync on it!

            await pipelineWithAiResponse.DeductPointsAsync();
            var finalPipeline = await pipelineWithAiResponse.ApplyToolCallsAsync();
            return finalPipeline.BuildFinalResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "An error occurred while processing your request. Please try again later.";
        }

        // The problem here is we still have to mess with the pipeline and we still have nesting in the try/catch
    }
}
