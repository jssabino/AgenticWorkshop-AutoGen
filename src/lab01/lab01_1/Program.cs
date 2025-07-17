using AutoGen.Core;
using AutoGen;
using Lab01_1.BasicSample;
using FluentAssertions;
using DotNetEnv;

// Load environment variables from .env file
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
Env.Load(dotenv);

var gpt4o = LLMConfiguration.GetAzureOpenAIGPT4o();
var config = new ConversableAgentConfig
{
    Temperature = 0,
    ConfigList = [gpt4o],
};

// create assistant agent
var assistantAgent = new AssistantAgent(
    name: "assistant",
    systemMessage: "You convert what user said to all uppercase.",
    llmConfig: config)
    .RegisterPrintMessage();

// talk to the assistant agent
var reply = await assistantAgent.SendAsync("hello world");
reply.Should().BeOfType<TextMessage>();
reply.GetContent().Should().Be("HELLO WORLD");

// to carry on the conversation, pass the previous conversation history to the next call
var conversationHistory = new List<IMessage>
{
    new TextMessage(Role.User, "hello world"), // first message
    reply, // reply from assistant agent
};

reply = await assistantAgent.SendAsync("hello world again", conversationHistory);
reply.Should().BeOfType<TextMessage>();
reply.GetContent().Should().Be("HELLO WORLD AGAIN");