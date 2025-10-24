using CsharpFunctionalApi.Services;

namespace CsharpFunctionalApi.Orchestrator.V2.V0;

public record CoreContext
{
    public List<LlmMessage> Messages = [];
    public LlmResult? AiResponseResult = null;
    public string FinalResult = "";
}
