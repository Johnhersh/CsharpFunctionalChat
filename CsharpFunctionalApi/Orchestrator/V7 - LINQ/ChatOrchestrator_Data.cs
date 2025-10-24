using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V7;

public record CoreContext
{
    public required Session Session;
}

public record PipelineStart();

public record PipelineData(CoreContext Core);

public record PipelineDataWithMessages(CoreContext Core, List<LlmMessage> Messages);

public record PipelineWithAiResponse(CoreContext Core, LlmResult AiResponseResult);

public record PipelineWithFinalResult(CoreContext Core, LlmResult AiResponseResult, string FinalResult);
