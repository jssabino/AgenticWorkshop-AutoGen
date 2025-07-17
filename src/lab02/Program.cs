using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Text;
using System.Text.Json;
using DotNetEnv;
using AutoGen;
using AutoGen.Core;
using AutoGen.AzureAIInference;
using AutoGen.AzureAIInference.Extension;
using Azure.AI.Inference;

// Main program
class Program
{
    static async Task Main(string[] args)
    {

        // Load environment variables from .env file
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");
        Env.Load(dotenv);

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.Configure<WebBrowsingConfig>(
                    context.Configuration.GetSection("WebBrowsing"));
                
                services.AddSingleton<ChatCompletionsClient>(provider =>
                {
                    var _apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
                        ?? throw new InvalidOperationException("OpenAI API key not found.");
                    var _apiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                        ?? throw new InvalidOperationException("OpenAI API endpoint not found.");

                    var _client = new ChatCompletionsClient(new Uri(_apiEndpoint), new Azure.AzureKeyCredential(_apiKey));
        
                    return _client;
                });
                
                services.AddSingleton<WebBrowsingAgentTeam>();
            })
            .Build();

        var webBrowsingConfig = host.Services.GetRequiredService<IOptions<WebBrowsingConfig>>().Value;
        var agentTeam = host.Services.GetRequiredService<WebBrowsingAgentTeam>();

        Console.WriteLine("🌐 WebBrowsing Agent Team - Lab 02");
        Console.WriteLine("==================================");
        Console.WriteLine();

        try
        {
            // Example 1: Research scenario
            Console.WriteLine("📚 Example 1: Research Scenario");
            Console.WriteLine("-------------------------------");
            
            var topic = "Microsoft AutoGen framework";
            var urls = new[]
            {
                "https://github.com/microsoft/autogen",
                "https://microsoft.github.io/autogen/"
            };

            Console.WriteLine($"🔍 Researching topic: {topic}");
            Console.WriteLine($"📍 Sources: {string.Join(", ", urls)}");
            Console.WriteLine();

            var result = await agentTeam.ResearchTopicAsync(topic, urls);
            
            Console.WriteLine("📊 Research Summary:");
            Console.WriteLine("===================");
            Console.WriteLine(result);
            Console.WriteLine();

            // Example 2: Group chat scenario
            Console.WriteLine("💬 Example 2: Group Chat Scenario");
            Console.WriteLine("----------------------------------");
            
            var message = "Please research information about https://github.com/microsoft/autogen and provide insights";
            var groupResult = await agentTeam.ProcessWithGroupChatAsync(message);
            
            Console.WriteLine("🤖 Group Chat Result:");
            Console.WriteLine("====================");
            Console.WriteLine(groupResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            await agentTeam.DisposeAsync();
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
