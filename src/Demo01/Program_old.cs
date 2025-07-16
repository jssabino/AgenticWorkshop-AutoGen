
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotNetEnv;

namespace Lab01;

public class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load();
        
        Console.WriteLine("🚀 AutoGen Lab 01: Introduction to AutoGen");
        Console.WriteLine("==========================================");
        
        try
        {
            // Configure services
            var host = CreateHostBuilder(args).Build();
            
            // Get the AutoGen service
            var autoGenService = host.Services.GetRequiredService<AutoGenService>();
            
            // Run the demo
            await autoGenService.RunIntroductionDemo();
            
            // Interactive mode
            await autoGenService.RunInteractiveMode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("\n💡 Make sure you have set your OpenAI API key in the .env file");
        }
        
        Console.WriteLine("\n👋 Thanks for trying AutoGen Lab 01!");
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<AutoGenService>();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}

public class AutoGenService
{
    private readonly ILogger<AutoGenService> _logger;
    private readonly IConfiguration _configuration;
    
    public AutoGenService(ILogger<AutoGenService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task RunIntroductionDemo()
    {
        Console.WriteLine("\n🤖 Creating your first AutoGen agent...");
        
        // Get API key from environment
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not found. Please set OPENAI_API_KEY in your .env file.");
        }
        
        // Create the assistant agent
        var assistantAgent = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", apiKey),
            name: "Assistant",
            systemMessage: """
                You are a helpful AI assistant created for the AutoGen workshop.
                You are friendly, knowledgeable, and excited to help users learn about AutoGen.
                Keep your responses concise but informative.
                When introducing yourself, mention that you're part of Lab 01 of the AutoGen workshop.
                """
        );
        
        // Create a user proxy agent
        var userProxy = new UserProxyAgent(
            name: "User",
            humanInputMode: HumanInputMode.NEVER
        );
        
        Console.WriteLine("✅ Agents created successfully!");
        Console.WriteLine("\n💬 Starting demonstration conversation...");
        
        // Demonstrate basic conversation
        var messages = new List<string>
        {
            "Hello! Can you introduce yourself?",
            "What is AutoGen and why is it useful?",
            "Can you explain what an agent is in the context of AutoGen?"
        };
        
        foreach (var message in messages)
        {
            Console.WriteLine($"\n👤 User: {message}");
            
            // Send message to assistant
            var response = await assistantAgent.GenerateReplyAsync(
                new List<IMessage> { new TextMessage(Role.User, message) }
            );
            
            Console.WriteLine($"🤖 Assistant: {response.GetContent()}");
            
            // Small delay for better readability
            await Task.Delay(1000);
        }
        
        Console.WriteLine("\n✨ Demonstration complete!");
    }
    
    public async Task RunInteractiveMode()
    {
        Console.WriteLine("\n🎮 Interactive Mode");
        Console.WriteLine("===================");
        Console.WriteLine("💡 You can now chat with your AutoGen agent!");
        Console.WriteLine("💡 Type 'quit' or 'exit' to end the conversation");
        
        // Get API key from environment
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        
        // Create the assistant agent with more interactive personality
        var assistantAgent = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", apiKey),
            name: "InteractiveAssistant",
            systemMessage: """
                You are an interactive AI assistant for the AutoGen workshop.
                You help users understand AutoGen concepts through conversation.
                You can discuss:
                - AutoGen architecture and components
                - Agent types and their roles
                - Multi-agent communication patterns
                - Best practices for agent design
                - Troubleshooting common issues
                
                Be conversational, helpful, and encourage questions.
                If users ask about advanced topics, provide good explanations but also suggest they continue to the next labs.
                """
        );
        
        var conversationHistory = new List<IMessage>();
        
        while (true)
        {
            Console.Write("\n👤 You: ");
            var userInput = Console.ReadLine();
            
            if (string.IsNullOrEmpty(userInput) || 
                userInput.ToLower() is "quit" or "exit" or "bye")
            {
                Console.WriteLine("👋 Goodbye! Continue to Lab 02 when you're ready!");
                break;
            }
            
            try
            {
                // Add user message to history
                var userMessage = new TextMessage(Role.User, userInput);
                conversationHistory.Add(userMessage);
                
                // Get response from assistant
                var response = await assistantAgent.GenerateReplyAsync(conversationHistory);
                
                Console.WriteLine($"🤖 Assistant: {response.GetContent()}");
                
                // Add assistant response to history
                conversationHistory.Add(response);
                
                // Keep conversation history manageable (last 10 messages)
                if (conversationHistory.Count > 10)
                {
                    conversationHistory.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine("💡 Please try again or check your API configuration.");
            }
        }
    }
}

// Helper classes for the demo
public class TextMessage : IMessage
{
    public Role Role { get; }
    public string Content { get; }
    
    public TextMessage(Role role, string content)
    {
        Role = role;
        Content = content;
    }
    
    public string GetContent() => Content;
}

public class UserProxyAgent
{
    public string Name { get; }
    public HumanInputMode HumanInputMode { get; }
    
    public UserProxyAgent(string name, HumanInputMode humanInputMode)
    {
        Name = name;
        HumanInputMode = humanInputMode;
    }
}

public enum HumanInputMode
{
    ALWAYS,
    NEVER,
    TERMINATE
}

public enum Role
{
    User,
    Assistant,
    System
}
