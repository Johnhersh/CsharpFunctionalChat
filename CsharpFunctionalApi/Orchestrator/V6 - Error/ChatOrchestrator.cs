using FluentResults;
using FluentResults.Extensions;

namespace CsharpFunctionalApi.Orchestrator.V6;

public record Session(User Player, int SessionId);
public record User(int Id, string Name);

public static class ChatOrchestrator
{
    public static async Task<string> ProcessMessages(Session session)
    {
        var startPipeline = new PipelineStart();

        var result = await Result.Ok(startPipeline)
            .Bind(p => p.EnsureSufficientPointsAsync(session))
            .Bind(p => p.BuildMessagesAsync())
            .Bind(p => p.GenerateAiResponseAsync())
            .Bind(p => p.DeductPointsAsync())
            .Bind(p => p.ApplyToolCallsAsync())
            .Bind(p => p.BuildFinalResult());

        return result.IsSuccess
            ? result.Value
            : result.Errors.First().Message;
    }
}
