using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V8;

public record PipelineStart();

public record PipelineData(Session Session);

public record PipelineDataWithMessages(Session Session, List<LlmMessage> Messages);

public record PipelineWithAiResponse(Session Session, LlmResult AiResponseResult);

public record PipelineWithFinalResult(Session Session, LlmResult AiResponseResult, string FinalResult);
