using CsharpFunctionalApi.Services;
using FluentResults;

namespace CsharpFunctionalApi.Orchestrator.V8;


public static class MessagesFunctions
{
    public static async Task<Result<PipelineDataWithMessages>> BuildMessagesAsync(this PipelineData data) =>
        Result.Ok(new PipelineDataWithMessages(
            data.Session,
            new[] { new LlmMessage("system", "Say hello to the user.") }
                .Concat(
                    (await DatabaseFunctions.GetSessionMessagesAsync(data.Session.SessionId))
                        ?.Select(m => new LlmMessage(m.Role == Role.User ? "user" : "assistant", m.Content))
                    ?? Enumerable.Empty<LlmMessage>()
                )
                .Append(new LlmMessage("user", "Hello!"))
                .ToList()
        ));
}

public static class LlmFunctions
{
    public static async Task<Result<PipelineWithAiResponse>> GenerateAiResponseAsync(this PipelineDataWithMessages data)
    {
        // Submit to the LLM
        var aiResponseResult = await LlmService.GenerateResponseAsync(data.Messages);
        return Result.Ok(new PipelineWithAiResponse(data.Session, aiResponseResult));
    }

    public static async Task<Result<PipelineWithFinalResult>> ApplyToolCallsAsync(this PipelineWithAiResponse data)
    {
        // Check for function call
        var content = data.AiResponseResult.Response.Content;
        var toolCallResult = await LlmService.DoToolCall(content);

        return Result.Ok(new PipelineWithFinalResult(
            data.Session,
            data.AiResponseResult,
            toolCallResult.tool switch
            {
                Tool.AddEmoji => $"{content} 😊",
                Tool.AddExclamation => $"{content} !",
                Tool.ToUpperCase => content.ToUpper(),
            }));
    }
}


public static class BusinessFunctions
{
    public static async Task<Result<PipelineWithAiResponse>> DeductPointsAsync(this PipelineWithAiResponse data)
    {
        // Deduct points only after successful response generation
        await EntitlementService.DeductPointsAsync(data.Session.Player.Id, data.AiResponseResult.TokensUsed);
        return Result.Ok(data);
    }

    public static Result<string> BuildFinalResult(this PipelineWithFinalResult data)
    {
        // Build final string to return
        var result = $"""
            {data.Session.Player.Name}!
            {data.FinalResult}

            """;

        return Result.Ok(result);
    }

    public static async Task<Result<PipelineData>> EnsureSufficientPointsAsync(this PipelineStart pipelineStart, Session session)
    {
        var hasPoints = await EntitlementService.HasSufficientPointsAsync(session.Player.Id, 1);

        return hasPoints ? Result.Ok(new PipelineData(session)) : Result.Fail("Insufficient points. Please upgrade your subscription to continue chatting.");
    }
}
