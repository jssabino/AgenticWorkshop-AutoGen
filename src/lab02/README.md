# Lab 02: WebBrowsing Agent Team

**Duration**: 60 minutes

## 🎯 Learning Objectives

By the end of this lab, you will be able to:

1. **Create a WebBrowsing Agent Team** using multimodalWebSurfer and RoundRobinGroupChat
2. **Integrate Playwright** for web automation and scraping
3. **Implement multi-modal content processing** (text, images, PDFs)
4. **Configure group chat dynamics** with multiple specialized agents
5. **Handle real-world web browsing scenarios** and data extraction

## 🛠 What You'll Build

In this lab, you'll create a sophisticated web browsing agent team that can:
- Navigate websites autonomously using Playwright
- Extract and process multi-modal content (text, images, documents)
- Coordinate between multiple specialized agents
- Conduct research and summarize findings
- Handle complex web interactions and form submissions

## 🔧 Prerequisites

- Completed Lab 01: Introduction to AutoGen
- Basic understanding of web technologies (HTML, CSS, JavaScript)
- Familiarity with web scraping concepts

## 📋 Lab Structure

### Part 1: Setting Up the Environment (15 minutes)
- Install Playwright and required packages
- Configure multimodalWebSurfer
- Set up browser automation

### Part 2: Creating the WebBrowsing Agent (20 minutes)
- Build the primary web browsing agent
- Configure Playwright integration
- Implement basic navigation and content extraction

### Part 3: Building the Agent Team (15 minutes)
- Create specialized agents (Researcher, Analyzer, Summarizer)
- Set up RoundRobinGroupChat
- Configure agent roles and responsibilities

### Part 4: Advanced Web Interactions (10 minutes)
- Handle forms and user interactions
- Process multi-modal content
- Implement error handling and retry logic

## 🚀 Getting Started

### Step 1: Environment Setup

First, let's install the required packages:

```bash
dotnet add package Microsoft.Playwright
dotnet add package Microsoft.Playwright.NUnit
dotnet add package PuppeteerSharp
dotnet add package HtmlAgilityPack
```

Install Playwright browsers:
```bash
npx playwright install
```

### Step 2: Project Configuration

Update your `appsettings.json` to include web browsing configuration:

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4o",
    "BaseUrl": "https://api.openai.com/v1"
  },
  "WebBrowsing": {
    "HeadlessMode": true,
    "DefaultTimeout": 30000,
    "MaxRetries": 3,
    "AllowedDomains": [
      "wikipedia.org",
      "github.com",
      "stackoverflow.com",
      "docs.microsoft.com"
    ]
  },
  "Agents": {
    "WebBrowser": {
      "Name": "WebBrowsingAgent",
      "Description": "Specializes in web navigation and content extraction",
      "MaxTokens": 2000
    },
    "Researcher": {
      "Name": "ResearchAgent",
      "Description": "Analyzes and processes web content for research purposes",
      "MaxTokens": 1500
    },
    "Summarizer": {
      "Name": "SummarizerAgent",
      "Description": "Creates concise summaries of research findings",
      "MaxTokens": 1000
    }
  }
}
```

### Step 3: Creating the WebBrowsing Agent

```csharp
using Microsoft.AutoGen.Agents;
using Microsoft.Playwright;
using System.Text.Json;

public class WebBrowsingAgent : IAgent
{
    private readonly IOpenAIClient _openAIClient;
    private readonly IPlaywright _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private readonly WebBrowsingConfig _config;

    public WebBrowsingAgent(IOpenAIClient openAIClient, WebBrowsingConfig config)
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
        
        // Set default timeout
        _page.SetDefaultTimeout(_config.DefaultTimeout);
    }

    public async Task<string> NavigateToUrlAsync(string url)
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized");

        try
        {
            await _page.GotoAsync(url);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Extract page content
            var title = await _page.TitleAsync();
            var content = await _page.InnerTextAsync("body");
            
            return $"Page Title: {title}\nURL: {url}\nContent: {content}";
        }
        catch (Exception ex)
        {
            return $"Error navigating to {url}: {ex.Message}";
        }
    }

    public async Task<string> ExtractContentAsync(string selector)
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized");

        try
        {
            var element = await _page.QuerySelectorAsync(selector);
            return element != null ? await element.InnerTextAsync() : "Element not found";
        }
        catch (Exception ex)
        {
            return $"Error extracting content: {ex.Message}";
        }
    }

    public async Task<byte[]> TakeScreenshotAsync()
    {
        if (_page == null)
            throw new InvalidOperationException("Browser not initialized");

        return await _page.ScreenshotAsync();
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser.Dispose();
        }
        _playwright?.Dispose();
    }
}
```

### Step 4: Building the Agent Team

```csharp
using Microsoft.AutoGen.Agents;

public class WebBrowsingAgentTeam
{
    private readonly IOpenAIClient _openAIClient;
    private readonly WebBrowsingAgent _webBrowsingAgent;
    private readonly ResearchAgent _researchAgent;
    private readonly SummarizerAgent _summarizerAgent;
    private readonly RoundRobinGroupChat _groupChat;

    public WebBrowsingAgentTeam(IOpenAIClient openAIClient, WebBrowsingConfig config)
    {
        _openAIClient = openAIClient;
        _webBrowsingAgent = new WebBrowsingAgent(openAIClient, config);
        _researchAgent = new ResearchAgent(openAIClient);
        _summarizerAgent = new SummarizerAgent(openAIClient);
        
        // Create group chat with round-robin orchestration
        _groupChat = new RoundRobinGroupChat(new[]
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
        
        await _webBrowsingAgent.DisposeAsync();
        
        return summary;
    }
}
```

### Step 5: Specialized Agents

```csharp
public class ResearchAgent : IAgent
{
    private readonly IOpenAIClient _openAIClient;

    public ResearchAgent(IOpenAIClient openAIClient)
    {
        _openAIClient = openAIClient;
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

        var response = await _openAIClient.GetChatCompletionAsync(prompt);
        return response.Content;
    }
}

public class SummarizerAgent : IAgent
{
    private readonly IOpenAIClient _openAIClient;

    public SummarizerAgent(IOpenAIClient openAIClient)
    {
        _openAIClient = openAIClient;
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

        var response = await _openAIClient.GetChatCompletionAsync(prompt);
        return response.Content;
    }
}
```

### Step 6: Main Program

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.Configure<WebBrowsingConfig>(
                    context.Configuration.GetSection("WebBrowsing"));
                services.AddSingleton<IOpenAIClient, OpenAIClient>();
                services.AddSingleton<WebBrowsingAgentTeam>();
            })
            .Build();

        var agentTeam = host.Services.GetRequiredService<WebBrowsingAgentTeam>();

        Console.WriteLine("🌐 WebBrowsing Agent Team - Lab 02");
        Console.WriteLine("==================================");
        Console.WriteLine();

        // Example research scenario
        var topic = "Microsoft AutoGen framework";
        var urls = new[]
        {
            "https://github.com/microsoft/autogen",
            "https://microsoft.github.io/autogen/",
            "https://arxiv.org/abs/2308.08155"
        };

        Console.WriteLine($"🔍 Researching topic: {topic}");
        Console.WriteLine($"📍 Sources: {string.Join(", ", urls)}");
        Console.WriteLine();

        try
        {
            var result = await agentTeam.ResearchTopicAsync(topic, urls);
            
            Console.WriteLine("📊 Research Summary:");
            Console.WriteLine("===================");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
```

## 🎯 Exercises

### Exercise 1: Custom Web Scraper
Create a specialized agent that can:
- Navigate to a specific website
- Extract product information
- Compare prices across multiple sites
- Generate a comparison report

### Exercise 2: Multi-Modal Content Processing
Extend the web browsing agent to:
- Process images found on web pages
- Extract text from PDFs
- Handle video content metadata
- Create rich media summaries

### Exercise 3: Form Interaction
Build an agent that can:
- Fill out web forms automatically
- Submit search queries
- Handle authentication flows
- Process form validation errors

### Exercise 4: Real-Time Web Monitoring
Create a monitoring system that:
- Tracks changes on specific web pages
- Sends alerts when content changes
- Maintains a history of changes
- Generates trend reports

## 🔍 Deep Dive: RoundRobinGroupChat

The RoundRobinGroupChat orchestrates multiple agents in a predictable sequence:

```csharp
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
        
        // Each agent processes in order
        for (int i = 0; i < _agents.Length; i++)
        {
            var agent = _agents[_currentAgentIndex];
            var result = await agent.ProcessMessageAsync(message);
            results.Add(result);
            
            _currentAgentIndex = (_currentAgentIndex + 1) % _agents.Length;
            
            // Use previous agent's output as input for next agent
            message = result;
        }
        
        return string.Join("\n---\n", results);
    }
}
```

## 🛠 Configuration Models

```csharp
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
```

## 🐛 Troubleshooting

### Common Issues

1. **Playwright Installation**
   ```bash
   # If browsers aren't installing
   npx playwright install --with-deps
   ```

2. **Timeout Issues**
   ```csharp
   // Increase timeout for slow pages
   _page.SetDefaultTimeout(60000);
   ```

3. **Memory Issues**
   ```csharp
   // Dispose of browser instances properly
   await _browser.CloseAsync();
   _browser.Dispose();
   ```

## 📚 Further Reading

- [Playwright Documentation](https://playwright.dev/dotnet/)
- [AutoGen Multi-Agent Patterns](https://microsoft.github.io/autogen/docs/topics/groupchat/intro)
- [Web Scraping Best Practices](https://scrapeops.io/web-scraping-playbook/)
- [Multi-Modal AI Applications](https://openai.com/research/gpt-4v-system-card)

## 🎯 Key Takeaways

- **WebBrowsing agents** can automate complex web interactions
- **RoundRobinGroupChat** provides predictable agent orchestration
- **Playwright integration** enables robust web automation
- **Multi-modal processing** handles diverse content types
- **Error handling** is crucial for reliable web scraping

## 📈 Next Steps

Ready to move on? Head to [Lab 03: Multi-Agent Systems](../lab03/README.md) where you'll explore advanced multi-agent coordination patterns and conversation management.

---

**🎉 Congratulations!** You've successfully built a WebBrowsing Agent Team with multi-modal capabilities and sophisticated orchestration patterns.
