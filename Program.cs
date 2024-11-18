using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using BlazorAIChatBot_with_AzureOpenAI.Components;
using BlazorAIChatBotOpenAI.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add logging with detailed information
//builder.Services.AddLogging();
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.AddEventSourceLogger();
//builder.Logging.AddFilter("Microsoft", LogLevel.Information);
//builder.Logging.AddFilter("System", LogLevel.Information);
//builder.Logging.AddFilter("BlazorChatBot", LogLevel.Debug);

builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});

// Register the chat client for Ollama
//builder.Services.AddSingleton<IChatClient>(static serviceProvider =>
//{
//    var logger = serviceProvider.GetRequiredService<ILogger>();

//    // Set up the Ollama connection string and default model
//    var ollamaCnnString = "http://localhost:11434";
//    var defaultLLM = "phi3:latest"; 

//    logger.LogInformation("Ollama connection string: {0}", ollamaCnnString);
//    logger.LogInformation("Default LLM: {0}", defaultLLM);

//    // Create the Ollama chat client with the updated connection URI and model name
//    IChatClient chatClient = new OllamaChatClient(new Uri(ollamaCnnString), defaultLLM);

//    return chatClient;
//});

// Register the chat client for Azure OpenAI
builder.Services.AddSingleton<IChatClient>(static serviceProvider =>
{
    var endpoint = new Uri("https://myopenaiserviceluis.openai.azure.com/");
    var credentials = new AzureKeyCredential("");
    var deploymentName = "gpt-4o";

    IChatClient client = new AzureOpenAIClient(endpoint, credentials).AsChatClient(deploymentName);

    IChatClient chatClient = new ChatClientBuilder()
        .UseFunctionInvocation()
        .Use(client);

    //IChatClient chatClient = new AzureOpenAIClient(
    //         new Uri("https://myopenaiserviceluis.openai.azure.com/"),
    //         new DefaultAzureCredential())
    //         .AsChatClient(modelId: "gpt-4o");

    return chatClient;
});

// Register default chat messages
builder.Services.AddSingleton<List<Microsoft.Extensions.AI.ChatMessage>>(static serviceProvider =>
{
    return new List<Microsoft.Extensions.AI.ChatMessage>()
    {
        new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, "You are a useful assistant that replies using short and precise sentences.")
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
