using CsharpFunctionalApi.Services;
using FluentResults;
using FluentResults.Extensions;

namespace CsharpFunctionalApi.Orchestrator.V5;

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
            // We move to railway oriented design. Now we can pipeline everything to the end.
            var pipeline = new PipelineData(new CoreContext { Session = session });

            var result = await Result.Ok(pipeline)
                .Bind(p => p.BuildMessagesAsync())
                .Bind(p => p.GenerateAiResponseAsync())
                .Bind(p => p.DeductPointsAsync())
                .Bind(p => p.ApplyToolCallsAsync())
                .Bind(p => p.BuildFinalResult());

            return result.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return "An error occurred while processing your request. Please try again later.";
        }

        // The last problem is now the try/catch
    }
}
