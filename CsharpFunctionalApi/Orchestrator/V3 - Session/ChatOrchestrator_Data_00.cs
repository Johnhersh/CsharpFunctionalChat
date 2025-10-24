using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V3;

public record CoreContext
{
    public required Session Session;
    public List<LlmMessage> Messages = [];
    public LlmResult? AiResponseResult = null;
    public string FinalResult = "";
}
