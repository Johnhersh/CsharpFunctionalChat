using CsharpFunctionalApi.Orchestrator.V1;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello", () =>
{
    return ChatOrchestrator.ProcessMessages(new Session(new User(1, "John"), 123));
});

app.Run();
