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

public class ResearchAgent : IAgent
{
    private readonly ChatCompletionsClient _openAIClient;
    private readonly ChatCompletionsClientAgent _agent;

    public string Name { get; set; } = "ResearchAgent";
    public ResearchAgent(ChatCompletionsClient openAIClient)
    {
        _openAIClient = openAIClient;

        _agent = new ChatCompletionsClientAgent(
            chatCompletionsClient: _openAIClient,
            modelName: "gpt-4o",
            name: "ResearchAgent",
            systemMessage: "Your task is to analyze research data and provide insights."
        );
    }

    
    public async Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return new TextMessage(Role.Assistant, await AnalyzeContentAsync(messages.FirstOrDefault()?.GetContent() ?? "", "general research"), from: "ResearchAgent");
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
            var response = await _agent.GenerateReplyAsync(
                new List<IMessage> { new TextMessage(Role.User, prompt, from: "user") }
            );

            return response?.GetContent() ?? "No analysis generated.";
        }
        catch (Exception ex)
        {
            return $"Error analyzing content: {ex.Message}";
        }
    }
}
