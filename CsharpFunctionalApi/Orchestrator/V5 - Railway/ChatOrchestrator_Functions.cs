using CsharpFunctionalApi.Services;
using FluentResults;

namespace CsharpFunctionalApi.Orchestrator.V5;


public static class MessagesFunctions
{
    public static async Task<Result<PipelineDataWithMessages>> BuildMessagesAsync(this PipelineData data)
    {
        var messages = new List<LlmMessage>();

        // Add system message first
        var systemPrompt = "Say hello to the user.";
        messages.Add(new LlmMessage("system", systemPrompt));

        // Generate AI response FIRST using existing conversation history and current user message
        var conversationHistory = await DatabaseFunctions.GetSessionMessagesAsync(data.Core.Session.SessionId);
        foreach (var msg in conversationHistory)
        {
            var role = msg.Role == Role.User ? "user" : "assistant";
            messages.Add(new LlmMessage(role, msg.Content));
        }

        // Add current user message
        var userMessage = new LlmMessage("user", "Hello!");
        messages.Add(userMessage);
        return Result.Ok(new PipelineDataWithMessages(data.Core, messages));
    }
}

public static class LlmFunctions
{
    public static async Task<Result<PipelineWithAiResponse>> GenerateAiResponseAsync(this PipelineDataWithMessages data)
    {
        // Submit to the LLM
        var aiResponseResult = await LlmService.GenerateResponseAsync(data.Messages);
        return Result.Ok(new PipelineWithAiResponse(data.Core, aiResponseResult));
    }

    public static async Task<Result<PipelineWithFinalResult>> ApplyToolCallsAsync(this PipelineWithAiResponse data)
    {
        // Check for function call
        var finalResult = data.AiResponseResult.Response.Content;
        var toolCallResult = await LlmService.DoToolCall(finalResult);
        if (toolCallResult.tool == Tool.AddEmoji) finalResult = $"{finalResult} 😊";
        if (toolCallResult.tool == Tool.AddExclamation) finalResult = $"{finalResult} !";
        return Result.Ok(new PipelineWithFinalResult(data.Core, data.AiResponseResult, finalResult));
    }
}


public static class BusinessFunctions
{
    public static async Task<Result<PipelineWithAiResponse>> DeductPointsAsync(this PipelineWithAiResponse data)
    {
        // Deduct points only after successful response generation
        await EntitlementService.DeductPointsAsync(data.Core.Session.Player.Id, data.AiResponseResult.TokensUsed);
        return Result.Ok(data);
    }

    public static Result<string> BuildFinalResult(this PipelineWithFinalResult data)
    {
        // Build final string to return
        var result = $"""
            {data.Core.Session.Player.Name}!
            {data.FinalResult}

            """;

        return Result.Ok(result);
    }
}
