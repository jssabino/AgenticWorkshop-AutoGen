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

public class SummarizerAgent : IAgent
{
    private readonly ChatCompletionsClient _openAIClient;
    private readonly ChatCompletionsClientAgent _agent;

    public string Name { get; set; } = "SummarizerAgent";

    public SummarizerAgent(ChatCompletionsClient openAIClient)
    {
        _openAIClient = openAIClient;

        _agent = new ChatCompletionsClientAgent(
            chatCompletionsClient: _openAIClient,
            modelName: "gpt-4o",
            name: "SummarizerAgent",
            systemMessage: "Your task is to summarize research findings concisely and accurately."
        );
    }

    public async Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return new TextMessage(Role.Assistant, await SummarizeAsync(messages.FirstOrDefault()?.GetContent() ?? ""), from: "SummarizerAgent");
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
            var response = await _agent.GenerateReplyAsync(
                new List<IMessage> {
                    new TextMessage(Role.User, prompt, from: "user")
                }
            );

            return response?.GetContent() ?? "No summary generated.";
        }
        catch (Exception ex)
        {
            return $"Error summarizing content: {ex.Message}";
        }
    }
}
