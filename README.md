# Integrating AI ChatBot with Blazor Web App Using .NET 9 AzureOpenAI or OpenAI GPT-4o Model

## 1. (OPTION 1) Get API Key and Service EndPoint from AzureOpenAI web page



## 1. (OPTION 2) Get API Key from OpenAI web page

We navigate to **OpenAI** web page and **Login**

![image](https://github.com/user-attachments/assets/fffa655c-b4c5-4664-b0a1-531844ba5e86)

Then we request a new **OpenAI API Key**

We click in **Settings** and then in **API Keys**

![image](https://github.com/user-attachments/assets/68507c4b-430f-4c8d-a7fe-0670756dc7ee)

Then we press on the **+ Create new secret key** button

![image](https://github.com/user-attachments/assets/ec2eb833-2d35-4818-b5f5-c76468109f3b)

We enter the API Key name and press **Create secret key** button

![image](https://github.com/user-attachments/assets/c14aec6c-4f1a-4f11-a3e1-24b14956eef3)

We copy the **API Key** value for using in our C# application

![image](https://github.com/user-attachments/assets/0fc7fa17-1c21-4048-b61e-f0465e019e1d)

## 2. Create a Blazor Web App (.NET 9)

We run Visual Studio 2022 Community Edition and we Create a new Project

![image](https://github.com/user-attachments/assets/50ab5224-a631-4e07-95ab-856640c91f83)

We select the Blazor Web App project template

![image](https://github.com/user-attachments/assets/91f9c737-5891-403d-8178-7e8fd100a8af)

We input the **project name and location** and we press the Next button

![image](https://github.com/user-attachments/assets/abda3cf7-abe9-40f4-a6bb-2f50886ea674)

We select the **.NET 9** framework and leave the other options with the default values, and we press the Create button

![image](https://github.com/user-attachments/assets/da2e303b-1947-43e9-bef7-0a02dc670f15)

We verify the project folders and files structure

![image](https://github.com/user-attachments/assets/8a18644b-abff-4181-9272-b0e27f0e154d)

## 3. Load the Nuget Packages

![image](https://github.com/user-attachments/assets/e7719162-e18e-4d75-98a6-7c5138f98bab)

**Azure.AI.OpenAI**: This library is part of the **Azure SDK**, specifically designed to interact with the **OpenAI models hosted on Azure**

It allows developers to leverage OpenAI's advanced **natural language models** (like **GPT**) for generating, understanding, and analyzing text

Common use cases include generating conversational responses, completing text, summarizing content, and more, directly integrated into Azure's cloud infrastructure

**Azure.Identity**: This library provides a unified way to handle **authentication** in Azure services

It includes various credential classes (e.g., **DefaultAzureCredential**, **ClientSecretCredential**) to authenticate applications to Azure services securely

It simplifies the process of accessing Azure resources by abstracting away the complexity of **handling tokens and secrets**

It supports **Azure Active Directory (Azure AD)** authentication and also integrates with **Managed Identity** for Azure services

**Microsoft.Extensions.AI" Version**: This is a part of Microsoft's dependency injection and configuration system extensions

It likely includes integrations for incorporating AI capabilities (like accessing AI models) into .NET applications using standard extension patterns

Its purpose is to make integrating AI tools easier in modern .NET applications

**Microsoft.Extensions.AI.OpenAI**: This library builds upon Microsoft.Extensions.AI by specifically providing support for **OpenAI models**

It is intended for developers using Microsoft's extensions framework to seamlessly **include OpenAI capabilities in their .NET applications**

It ensures smooth integration of OpenAI's language models with dependency injection, configuration, and scalability features

**System.ComponentModel.Annotations**: This library is part of the **.NET Framework** and provides a way to define and enforce **metadata** for classes and properties using **attributes**

It is commonly used for: 

**Validation**: Apply attributes like [Required], [StringLength], and [Range] to **validate user input or object properties**

**Data Annotations**: Add metadata for describing and controlling **how data is displayed**, e.g., [Display(Name = "Username")].

**Model Binding**: Used in frameworks like ASP.NET Core to support **automatic model validation and binding**

This library is a cornerstone of model validation in .NET applications, especially in web development

## 4. Modify the middleware(Program.cs)

We first have to register the **Log Service**

```csharp
builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});
```

We register the **Chat Client Service** for Azure OpenAI

```csharp
builder.Services.AddSingleton<IChatClient>(static serviceProvider =>
{
    var endpoint = new Uri("https://myopenaiserviceluis.openai.azure.com/");
    var credentials = new AzureKeyCredential("");
    var deploymentName = "gpt-4o";

    IChatClient client = new AzureOpenAIClient(endpoint, credentials).AsChatClient(deploymentName);

    IChatClient chatClient = new ChatClientBuilder()
        .UseFunctionInvocation()
        .Use(client);

    return chatClient;
});
```

**IMPORTANT NOTE**: copy the **OpenAI API Key** form Section 1 and paste it in this code line

```csharp
var credentials = new AzureKeyCredential("API_Key_from_Section_1");
```

We also have to register the **Chat Messages Service**

```csharp
builder.Services.AddSingleton<List<ChatMessage>>(static serviceProvider =>
{
    return new List<ChatMessage>()
    {
        new ChatMessage(ChatRole.System, "You are a useful assistant that replies using short and precise sentences.")
    };
});
```

We verify the whole **Program.cs** file

```csharp
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
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

builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});

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
```

## 5. Add the Chatbot

We create a new folder **Chatbot** inside the **Components** folder

![image](https://github.com/user-attachments/assets/577166ba-2a16-4e71-ac29-8d0cba624895)

Then we are going to create the classes files and razor components

We first create the **ChatState.cs**. This file can be summarized in two lines:

```csharp
ChatMessages.Add(new ChatMessage(ChatRole.User, userText));
...
ChatMessages.Add(new ChatMessage(ChatRole.Assistant, $"My apologies, but I encountered an unexpected error.\n\n<p style=\"color: red\">{e}</p>"));
```

This is the whole **ChatState.cs** file:

```csharp
using Microsoft.Extensions.AI;
using System.Security.Claims;
using System.Text;

namespace BlazorAIChatBotOpenAI.Components.Chatbot;

public class ChatState
{
    private readonly ILogger _logger;
    private readonly IChatClient _chatClient;
    private List<ChatMessage> _chatMessages;

    public List<ChatMessage> ChatMessages { get => _chatMessages; set => _chatMessages = value; }

    public ChatState(ClaimsPrincipal user, IChatClient chatClient, List<ChatMessage> chatMessages, ILogger logger)
    {
        _logger = logger;
        _chatClient = chatClient;
        ChatMessages = chatMessages;
    }

    public async Task AddUserMessageAsync(string userText, Action onMessageAdded)
    {
        ChatMessages.Add(new ChatMessage(ChatRole.User, userText));
        onMessageAdded();

        try
        {
            _logger.LogInformation("Sending message to chat client.");
            _logger.LogInformation($"user Text: {userText}");

            var result = await _chatClient.CompleteAsync(ChatMessages);
            ChatMessages.Add(new ChatMessage(ChatRole.Assistant, result.Message.Text));
            
            _logger.LogInformation($"Assistant Response: {result.Message.Text}");
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Error getting chat completions.");
            }

            // format the exception using HTML to show the exception details in a chat panel as response
            ChatMessages.Add(new ChatMessage(ChatRole.Assistant, $"My apologies, but I encountered an unexpected error.\n\n<p style=\"color: red\">{e}</p>"));
        }
        onMessageAdded();
    }
}
```

We also have to create the **MessageProcessor.cs** file 

```csharp
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace BlazorAIChatBotOpenAI.Components.Chatbot;

public static partial class MessageProcessor
{
    public static MarkupString AllowImages(string message)
    {
        // Having to process markdown and deal with HTML encoding isn't ideal. If the language model could return
        // search results in some defined format like JSON we could simply loop over it in .razor code. This is
        // fine for now though.

        var result = new StringBuilder();
        var prevEnd = 0;
        message = message.Replace("&lt;", "<").Replace("&gt;", ">");

        foreach (Match match in FindMarkdownImages().Matches(message))
        {
            var contentToHere = message.Substring(prevEnd, match.Index - prevEnd);
            result.Append(HtmlEncoder.Default.Encode(contentToHere));
            result.Append($"<img title=\"{(HtmlEncoder.Default.Encode(match.Groups[1].Value))}\" src=\"{(HtmlEncoder.Default.Encode(match.Groups[2].Value))}\" />");

            prevEnd = match.Index + match.Length;
        }
        result.Append(HtmlEncoder.Default.Encode(message.Substring(prevEnd)));

        return new MarkupString(result.ToString());
    }

    public static MarkupString ProcessMessageToHTML(string message)
    {
        return new MarkupString(message);
    }

    [GeneratedRegex(@"\!?\[([^\]]+)\]\s*\(([^\)]+)\)")]
    private static partial Regex FindMarkdownImages();
}
```

Now we create the razor components:

We create the ShowChatbot button

**ShowChatbotButton.razor**

```razor
@inject NavigationManager Nav

<a class="show-chatbot" href="@Nav.GetUriWithQueryParameter("chat", true)" title="Show chatbot"></a>

@if (ShowChat)
{
    <Chatbot />
}

@code {
    [SupplyParameterFromQuery(Name = "chat")]
    public bool ShowChat { get; set; }
}
```

And also we create the Chatbot razor component

**Chatbot.razor**

```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
@using Microsoft.AspNetCore.Components.Authorization
@using BlazorAIChatBotOpenAI.Components.Chatbot
@using Microsoft.Extensions.AI
@inject IJSRuntime JS
@inject NavigationManager Nav

@inject AuthenticationStateProvider AuthenticationStateProvider
@inject ILogger Logger
@inject IConfiguration Configuration
@inject IServiceProvider ServiceProvider

<div class="floating-pane">
    <a href="@Nav.GetUriWithQueryParameter("chat", (string?)null)" class="hide-chatbot" title="Close Chat"><span>✖</span></a>

    <div class="chatbot-chat" @ref="chat">
        @if (chatState is not null)
        {
            foreach (var message in chatState.ChatMessages.Where(m => m.Role == ChatRole.Assistant || m.Role == ChatRole.User))
            {
                if (!string.IsNullOrEmpty(message.Contents[0].ToString()))
                {
                    <p @key="@message" class="message message-@message.Role">@MessageProcessor.AllowImages(message.Contents[0].ToString()!)</p>                    
                }
            }
        }
        else if (missingConfiguration)
        {
            <p class="message message-assistant"><strong>The chatbot is missing required configuration.</strong> Please review your app settings.</p>
        }

        @if (thinking)
        {
            <p class="thinking">"[phi3:latest]" is Thinking...</p>
        }

    </div>

    <form class="chatbot-input" @onsubmit="SendMessageAsync">
        <textarea placeholder="Start chatting..." @ref="@textbox" @bind="messageToSend"></textarea>
        <button type="submit" title="Send" disabled="@(chatState is null)">Send</button>
    </form>
</div>

@code {
    bool missingConfiguration;
    ChatState? chatState;
    ElementReference textbox;
    ElementReference chat;
    string? messageToSend;
    bool thinking;
    IJSObjectReference? jsModule;

    protected override async Task OnInitializedAsync()
    {
        IChatClient chatClient = ServiceProvider.GetService<IChatClient>();
        List<ChatMessage> chatMessages = ServiceProvider.GetService<List<ChatMessage>>();
        if (chatClient is not null)
        {
            AuthenticationState auth = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            chatState = new ChatState(auth.User, chatClient, chatMessages, Logger);
        }
        else
        {
            missingConfiguration = true;
        }
    }

    private async Task SendMessageAsync()
    {
        var messageCopy = messageToSend?.Trim();
        messageToSend = null;

        if (chatState is not null && !string.IsNullOrEmpty(messageCopy))
        {
            thinking = true;
            await chatState.AddUserMessageAsync(messageCopy, onMessageAdded: StateHasChanged);
            thinking = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Chatbot/Chatbot.razor.js");
        await jsModule.InvokeVoidAsync("scrollToEnd", chat);

        if (firstRender)
        {
            await textbox.FocusAsync();
            await jsModule.InvokeVoidAsync("submitOnEnter", textbox);
        }
    }
}
```

## 6. Modify the Home.razor component

We have to invoke the Show Chatbot button from the home page, for this purpose we add the following code:

**Home.razor**

```razor
@page "/"

@using BlazorAIChatBotOpenAI.Components.Chatbot
@using BlazorAIChatBot_with_AzureOpenAI.Components.Chatbot

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

Welcome to your new app.

<ShowChatbotButton />
```

## 7. Run the application a see the results

![image](https://github.com/user-attachments/assets/ca75a221-32ae-46b7-a223-ff3a22a0cf1d)

![image](https://github.com/user-attachments/assets/e530c46e-933b-474c-b41a-89df511030b5)

We write the message and press the Send button

![image](https://github.com/user-attachments/assets/db4f7c2a-eac2-450a-a761-d650854dd507)




