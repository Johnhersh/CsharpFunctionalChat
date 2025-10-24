### Show two graphics
On second one mention that the .filter() function is invisible to us

### Talk about get endpoint, use curl
curl -H "Accept: application/json" http://localhost:5191/hello

- Go to the Services folder, show base classes
- Show the ChatOrchestrator

Talk about ChatOrchestrator being imperative


### First fix is to apply code comments
[text](CsharpFunctionalApi/Orchestrator/V1/ChatOrchestrator_01_labels.cs)

Second is to break those sections into functions
[text](CsharpFunctionalApi/Orchestrator/V1/ChatOrchestrator_02_functions.cs)

The problem at this point is everything is still intermixed. It's actually a bit harder to read.
What we should strive for is separating behavior from data

## Make new ChatOrchestratorBehavior.cs file
[text](CsharpFunctionalApi/Orchestrator/V1/ChatOrchestrator_03_functions_external.cs)

The problem we have now is we've kinda separated behavior, and we're on our way towards
making the behavior invisible to us, but the caller still is exposed to the inner working
of the functions. It's still carrying data back and forth and kinda has to know what
they're doing. We need to properly separate the data


### Make new ChatOrchestratorData.cs file
[text](CsharpFunctionalApi/Orchestrator/V2/ChatOrchestrator_Data_00.cs)

Talk about context pattern. It's a stepping stone to other patterns. I've seen it used
To define more complicated flows.

Type it out manually. Skip making LlmResult null and show the problem with that.
This hints at what will happen next.
Forces me to deal with my bullshit because now I can see that I'm thinking about it wrong.
I can't have this data this early.
Ignore for a second and copy the behavior:
[text](CsharpFunctionalApi/Orchestrator/V2/ChatOrchestrator_00_context_external.cs)
This will work fine. I can talk about how they each take the context now

Copy the new orchestrator over:
[text](CsharpFunctionalApi/Orchestrator/V2/ChatOrchestrator_00_context.cs)
There won't be an error. Go add `required` to silence the warning and see the error
Make it nullable for now and continue


### Talk about the Session problem
We still have a problem in that the caller needs to supply session information
We deal with this by adding Session to the context:
[text](<CsharpFunctionalApi/Orchestrator/V3 - Session/ChatOrchestrator_Data_00.cs>)

Update the functions to take in the session:
[text](<CsharpFunctionalApi/Orchestrator/V3 - Session/ChatOrchestrator_00_session_external.cs>)

Update the orchestrator:
[text](<CsharpFunctionalApi/Orchestrator/V3 - Session/ChatOrchestrator_00_session.cs>)



### Pipeline
The main orchestrator looks much cleaner!
But there's still a problem with nullable values and data not being available when it should be
It's a scoping problem
The solution is to introduce the pipeline pattern:
[text](<CsharpFunctionalApi/Orchestrator/V4 - Pipeline/ChatOrchestrator_Data.cs>)
Maybe type this out manually? Show that each step adds the data from before.
Technically it could also take away data

Go to the functions:
[text](<CsharpFunctionalApi/Orchestrator/V4 - Pipeline/ChatOrchestrator_Functions.cs>)

Show how BuildMessagesAsync only needs PipelineData. Refactor it manually by replacing the input
with `PipelineData data`

Move to the orchestrator:
[text](<CsharpFunctionalApi/Orchestrator/V4 - Pipeline/ChatOrchestrator.cs>)

Type in the `var pipeline` and explain how we can now pass the pipeline down into the function

But! There's a problem. The orchestrator still works with an old paradigm where it's aware of
both data and behavior. That's not how a pipeline should work. It should be "pure" and pass
things to the next link in the chain. Go add the `this` keyword to `BuildMessagesAsync`

In the orchestrator show that we can call pipeline.BuildMessagesAsync() and the function is there
But it returns a new data type that doesn't have that code on it. So it's very strict on what can
be called on which data and when.

Copy the entire functions file and then the entire orchestrator file.
Talk about how now the orchestrator doesn't know anything about the data, doesn't know anything
about the behavior. They're isolated away in another place safely

Show how in theory we can already chain by wrapping in parenthesis `(await pipeline.BuildMessagesAsync()).GenerateAiResponseAsync();`

But this doesn't work well because of async and a couple of missing pieces


### Railway oriented programming

One fun solution that comes from the functional world is railway. It allows us to clearly define
the happy and fail paths.

Add the Result type to the first function:
[text](<CsharpFunctionalApi/Orchestrator/V5 - Railway/ChatOrchestrator_Functions.cs>)

Go back to the orchestrator and show that it broke because of the type of pipelineWithMessages

Copy the entire functions file so we have the full pipeline

Go to orchestrator
[text](<CsharpFunctionalApi/Orchestrator/V5 - Railway/ChatOrchestrator.cs>)

Show how we now have the .Bind() function we can use


### Error
We want to remove the points check and the try/catch. Go to the data and add the PipelineStart:
[text](<CsharpFunctionalApi/Orchestrator/V6 - Error/ChatOrchestrator_Data.cs>)

It needs nothing because it happens before anything

Add a new function at the end:
[text](<CsharpFunctionalApi/Orchestrator/V6 - Error/ChatOrchestrator_Functions.cs>)

Function:
[text](<CsharpFunctionalApi/Orchestrator/V6 - Error/ChatOrchestrator_Functions.cs>)

[text](<CsharpFunctionalApi/Orchestrator/V6 - Error/ChatOrchestrator.cs>)
