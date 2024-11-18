using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Security.Claims;
using System.Text;

namespace BlazorAIChatBotOpenAI.Components.Chatbot;

public class ChatState
{
    private readonly ILogger _logger;
    private readonly IChatClient? _chatClient;
    private List<Microsoft.Extensions.AI.ChatMessage>? _chatMessages;

    public List<Microsoft.Extensions.AI.ChatMessage>? ChatMessages { get => _chatMessages; set => _chatMessages = value; }

    public ChatState(ClaimsPrincipal user, IChatClient chatClient, List<Microsoft.Extensions.AI.ChatMessage>? chatMessages, ILogger logger)
    {
        _logger = logger;
        _chatClient = chatClient;
        ChatMessages = chatMessages;
    }

    public async Task AddUserMessageAsync(string userText, ChatOptions options, Action onMessageAdded)
    {
        ChatMessages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, userText));
        onMessageAdded();

        try
        {
            _logger.LogInformation("Sending message to chat client.");
            _logger.LogInformation($"user Text: {userText}");

            var result = await _chatClient.CompleteAsync(ChatMessages, options);
            ChatMessages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, result.Message.Text));
            
            _logger.LogInformation($"Assistant Response: {result.Message.Text}");
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Error getting chat completions.");
            }

            // format the exception using HTML to show the exception details in a chat panel as response
            ChatMessages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, $"My apologies, but I encountered an unexpected error.\n\n<p style=\"color: red\">{e}</p>"));
        }
        onMessageAdded();
    }
}