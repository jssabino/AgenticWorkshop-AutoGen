using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using OpenAI;
using System.Text;
using System.Text.Json;

// Configuration classes
public class WebBrowsingConfig
{
    public bool HeadlessMode { get; set; } = true;
    public int DefaultTimeout { get; set; } = 30000;
    public int MaxRetries { get; set; } = 3;
    public string[] AllowedDomains { get; set; } = Array.Empty<string>();
}

public class AgentConfig
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int MaxTokens { get; set; } = 1000;
}

public class OpenAIConfig
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gpt-4o";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}

// Agent interfaces and implementations
public interface IAgent
{
    Task<string> ProcessMessageAsync(string message);
}

public class WebBrowsingAgent : IAgent, IAsyncDisposable
{
    private readonly OpenAIClient _openAIClient;
    private readonly WebBrowsingConfig _config;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    public WebBrowsingAgent(OpenAIClient openAIClient, WebBrowsingConfig config)
    {
        _openAIClient = openAIClient;
        _config = config;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _config.HeadlessMode,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        });
        _page = await _browser.NewPageAsync();
        _page.SetDefaultTimeout(_config.DefaultTimeout);
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        // Extract URL from message if present
        var url = ExtractUrlFromMessage(message);
        if (!string.IsNullOrEmpty(url))
        {
            return await NavigateToUrlAsync(url);
        }
        
        return "No valid URL found in message for web browsing.";
    }

    private string ExtractUrlFromMessage(string message)
    {
        // Simple URL extraction - in real implementation, use regex or NLP
        var words = message.Split(' ');
        return words.FirstOrDefault(w => w.StartsWith("http")) ?? "";
    }

    public async Task<string> NavigateToUrlAsync(string url)
    {
        if (_page == null)
        {
            await InitializeAsync();
        }

        try
        {
            await _page!.GotoAsync(url);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            var title = await _page.TitleAsync();
            var content = await _page.InnerTextAsync("body");
            
            // Limit content length for processing
            if (content.Length > 2000)
            {
                content = content.Substring(0, 2000) + "...";
            }
            
            return $"Page Title: {title}\nURL: {url}\nContent: {content}";
        }
        catch (Exception ex)
        {
            return $"Error navigating to {url}: {ex.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser.Dispose();
        }
        _playwright?.Dispose();
    }
}

public class ResearchAgent : IAgent
{
    private readonly OpenAIClient _openAIClient;

    public ResearchAgent(OpenAIClient openAIClient)
    {
        _openAIClient = openAIClient;
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        return await AnalyzeContentAsync(message, "general research");
    }

    public async Task<string> AnalyzeContentAsync(string content, string topic)
    {
        var prompt = $@"
        As a research specialist, analyze the following content for information related to '{topic}'.
        
        Content: {content}
        
        Please provide:
        1. Key relevant information
        2. Important facts and figures
        3. Credibility assessment
        4. Relevance score (1-10)
        
        Focus on accuracy and relevance to the research topic.
        ";

        try
        {
            var response = await _openAIClient.GetChatCompletionAsync(
                new List<ChatMessage> { new UserChatMessage(prompt) }
            );
            
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            return $"Error analyzing content: {ex.Message}";
        }
    }
}

public class SummarizerAgent : IAgent
{
    private readonly OpenAIClient _openAIClient;

    public SummarizerAgent(OpenAIClient openAIClient)
    {
        _openAIClient = openAIClient;
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        return await SummarizeAsync(message);
    }

    public async Task<string> SummarizeAsync(string researchData)
    {
        var prompt = $@"
        As a summarization specialist, create a comprehensive summary of the following research data.
        
        Research Data: {researchData}
        
        Please provide:
        1. Executive summary (2-3 sentences)
        2. Key findings (bullet points)
        3. Sources overview
        4. Recommendations for further research
        
        Keep the summary concise but comprehensive.
        ";

        try
        {
            var response = await _openAIClient.GetChatCompletionAsync(
                new List<ChatMessage> { new UserChatMessage(prompt) }
            );
            
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            return $"Error summarizing content: {ex.Message}";
        }
    }
}

// Round Robin Group Chat implementation
public class RoundRobinGroupChat
{
    private readonly IAgent[] _agents;
    private int _currentAgentIndex = 0;

    public RoundRobinGroupChat(IAgent[] agents)
    {
        _agents = agents;
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        var results = new List<string>();
        var currentMessage = message;
        
        // Each agent processes in order
        for (int i = 0; i < _agents.Length; i++)
        {
            var agent = _agents[_currentAgentIndex];
            var result = await agent.ProcessMessageAsync(currentMessage);
            results.Add($"Agent {_currentAgentIndex + 1}: {result}");
            
            _currentAgentIndex = (_currentAgentIndex + 1) % _agents.Length;
            
            // Use previous agent's output as input for next agent
            currentMessage = result;
        }
        
        return string.Join("\n---\n", results);
    }
}

// Main agent team orchestrator
public class WebBrowsingAgentTeam : IAsyncDisposable
{
    private readonly WebBrowsingAgent _webBrowsingAgent;
    private readonly ResearchAgent _researchAgent;
    private readonly SummarizerAgent _summarizerAgent;
    private readonly RoundRobinGroupChat _groupChat;

    public WebBrowsingAgentTeam(OpenAIClient openAIClient, WebBrowsingConfig config)
    {
        _webBrowsingAgent = new WebBrowsingAgent(openAIClient, config);
        _researchAgent = new ResearchAgent(openAIClient);
        _summarizerAgent = new SummarizerAgent(openAIClient);
        
        _groupChat = new RoundRobinGroupChat(new IAgent[]
        {
            _webBrowsingAgent,
            _researchAgent,
            _summarizerAgent
        });
    }

    public async Task<string> ResearchTopicAsync(string topic, string[] urls)
    {
        await _webBrowsingAgent.InitializeAsync();
        
        var research = new StringBuilder();
        
        foreach (var url in urls)
        {
            Console.WriteLine($"🔍 Processing: {url}");
            
            // Navigate and extract content
            var content = await _webBrowsingAgent.NavigateToUrlAsync(url);
            
            // Process with research agent
            var analysis = await _researchAgent.AnalyzeContentAsync(content, topic);
            
            research.AppendLine($"Source: {url}");
            research.AppendLine($"Analysis: {analysis}");
            research.AppendLine();
        }
        
        // Summarize findings
        var summary = await _summarizerAgent.SummarizeAsync(research.ToString());
        
        return summary;
    }

    public async Task<string> ProcessWithGroupChatAsync(string message)
    {
        return await _groupChat.ProcessMessageAsync(message);
    }

    public async ValueTask DisposeAsync()
    {
        await _webBrowsingAgent.DisposeAsync();
    }
}

// Main program
class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.Configure<WebBrowsingConfig>(
                    context.Configuration.GetSection("WebBrowsing"));
                services.Configure<OpenAIConfig>(
                    context.Configuration.GetSection("OpenAI"));
                
                services.AddSingleton<OpenAIClient>(provider =>
                {
                    var config = provider.GetRequiredService<IOptions<OpenAIConfig>>().Value;
                    return new OpenAIClient(config.ApiKey);
                });
                
                services.AddSingleton<WebBrowsingAgentTeam>();
            })
            .Build();

        var agentTeam = host.Services.GetRequiredService<WebBrowsingAgentTeam>();
        var webBrowsingConfig = host.Services.GetRequiredService<IOptions<WebBrowsingConfig>>().Value;

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
