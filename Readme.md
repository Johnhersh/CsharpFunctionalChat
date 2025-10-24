This repo represents a progressive refactoring of imperative oop-ish code into more declarative funcional(very ish) code.

To start just run `dotnet watch` in the terminal.

You can ping the server with:

```sh
curl -H "Accept: application/json" http://localhost:5191/hello
```

There is also a `launch.json` file with a debugger configuration if you want to launch it from VSCode and set breakpoints.

First look in [Program.cs](CsharpFunctionalApi/Program.cs). Note the using statement at the top:

```cs
using CsharpFunctionalApi.Orchestrator.V1;
```

This activates the code from V1. Each version has its own `ChatOrchestrator.cs` but in a different namespace to prevent collisons.

Simply change the version in `Program.cs` to V2, V3, etc, to activate a different version. This is useful if you want to run the debugger and step through the code.
