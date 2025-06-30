
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();



async Task RunTest(KernelFunction[] functions, Dictionary<string, List<string>> todos)
{
    var kernelBuilder = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(config["deployment"], config["endpoint"], config["apiKey"], modelId: config["modelId"]);
    kernelBuilder.Plugins.AddFromFunctions("todos", functions);


    var kernel = kernelBuilder.Build();


    var completionService = kernel.GetRequiredService<IChatCompletionService>();

    var history = new ChatHistory();
    history.AddSystemMessage("You are an assistant that can manage use TODOs");
    history.AddUserMessage("Can you add 5 common items to my shopping todo list? Make sure you don't have duplicates, if there are any, tell me what they are. Provide feedback after every function call.");
    var completion = await completionService.GetChatMessageContentsAsync(history, new()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    }, kernel);

    Console.WriteLine("--");
    foreach (var message in completion)
        Console.WriteLine(message.Content);

    foreach (var list in todos)
    {
        Console.WriteLine(list.Key);
        foreach (var item in list.Value)
        {
            Console.WriteLine($"  - {item}");
        }
    }
}


var todos1 = new TodoState();
var todos2 = new TodoState();

Console.WriteLine("--KernelFunction--");
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
await RunTest([
        new DebugAIFunction( KernelFunctionFactory.CreateFromMethod(todos1.add_todos, new(){
            FunctionName = "add_todos",
            Description = "Add todo items to a specific scope/group"
        })).AsKernelFunction(),
        new DebugAIFunction(KernelFunctionFactory.CreateFromMethod(todos1.remove_todos, new(){
            FunctionName = "remove_todos",
            Description = "Remove todo items from a specific scope/group"
        })).AsKernelFunction(),
        new DebugAIFunction(KernelFunctionFactory.CreateFromMethod(todos1.get_todos, new(){
            FunctionName = "get_todos",
            Description = "List all todo items of a specific scope/group"
        })).AsKernelFunction(),
        new DebugAIFunction(KernelFunctionFactory.CreateFromMethod(todos1.get_scopes, new(){
            FunctionName = "get_Scopes",
            Description = "List all known scopes/groups"
        })).AsKernelFunction()
    ], todos1.todos);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

Console.WriteLine("--AIFunction--");
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
await RunTest([
        new DebugAIFunction(AIFunctionFactory.Create(todos2.add_todos, new(){
            Name = "add_todos",
            Description = "Add todo items to a specific scope/group"
        })).AsKernelFunction(),
        new DebugAIFunction(AIFunctionFactory.Create(todos2.remove_todos, new(){
            Name = "remove_todos",
            Description = "Remove todo items from a specific scope/group"
        })).AsKernelFunction(),
        new DebugAIFunction(AIFunctionFactory.Create(todos2.get_todos, new(){
            Name = "get_todos",
            Description = "List all todo items of a specific scope/group"
        })).AsKernelFunction(),
        new DebugAIFunction(AIFunctionFactory.Create(todos2.get_scopes, new(){
            Name = "get_Scopes",
            Description = "List all known scopes/groups"
        })).AsKernelFunction()
    ], todos2.todos);




Console.WriteLine("----Raw Calls----");
var todos3 = new TodoState();
var rawTest = new DebugAIFunction(AIFunctionFactory.Create(todos3.add_todos, new()
{
    Name = "add_todos",
    Description = "Add todo items to a specific scope/group"
}));
{
    Console.WriteLine("--objects--");
    var result = await rawTest.InvokeAsync(new(new Dictionary<string, object>()
    {
        ["items"] = new string[] { "bread", "chicken" },
        ["scope"] = "shopping"
    }));

    Console.WriteLine(result);
}

try
{
    Console.WriteLine("--JSON string--");
    var result = await rawTest.InvokeAsync(new(new Dictionary<string, object>()
    {
        ["items"] = "[ \"bread\", \"chicken\" ]",
        ["scope"] = "shopping"
    }));

    Console.WriteLine(result);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

try
{
    Console.WriteLine("--JsonElement--");
    var result = await rawTest.InvokeAsync(new(new Dictionary<string, object>()
    {
        ["items"] = JsonDocument.Parse("[ \"bread\", \"chicken\" ]").RootElement,
        ["scope"] = "shopping"
    }));

    Console.WriteLine(result);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}