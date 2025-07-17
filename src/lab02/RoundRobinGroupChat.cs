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
            var result = await agent.GenerateReplyAsync(
                new List<IMessage> { new TextMessage(Role.User, currentMessage, from: "user") });
            results.Add($"Agent {_currentAgentIndex + 1}: {result}");

            _currentAgentIndex = (_currentAgentIndex + 1) % _agents.Length;

            // Use previous agent's output as input for next agent
            currentMessage = result.GetContent() ?? string.Empty;
        }

        return string.Join("\n---\n", results);
    }
}
