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

public class WebBrowsingAgentTeam : IAsyncDisposable
{
    private readonly WebBrowsingAgent _webBrowsingAgent;
    private readonly ResearchAgent _researchAgent;
    private readonly SummarizerAgent _summarizerAgent;
    private readonly RoundRobinGroupChat _groupChat;

    public WebBrowsingAgentTeam(ChatCompletionsClient openAIClient, WebBrowsingConfig config)
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
