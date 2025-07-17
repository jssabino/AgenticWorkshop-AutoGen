// Copyright (c) Microsoft Corporation. All rights reserved.
// LLMConfiguration.cs

using AutoGen;
using AutoGen.OpenAI;

namespace Lab01_1.BasicSample;

internal static class LLMConfiguration
{
    public static AzureOpenAIConfig GetAzureOpenAIGPT3_5_Turbo(string deployName = "gpt-35-turbo-16k")
    {
        var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new Exception("Please set AZURE_OPENAI_API_KEY environment variable.");
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new Exception("Please set AZURE_OPENAI_ENDPOINT environment variable.");

        return new AzureOpenAIConfig(endpoint, deployName, azureOpenAIKey);
    }

    public static AzureOpenAIConfig GetAzureOpenAIGPT4o(string deployName = "gpt-4o")
    {
        var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new Exception("Please set AZURE_OPENAI_API_KEY environment variable.");
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new Exception("Please set AZURE_OPENAI_ENDPOINT environment variable.");

        return new AzureOpenAIConfig(endpoint, deployName, azureOpenAIKey);
    }
}