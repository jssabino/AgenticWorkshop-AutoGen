using AutoGen;
using AutoGen.Core;
using Lab03.BasicSample;
using FluentAssertions;
using DotNetEnv;

// Load environment variables from .env file
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
Env.Load(dotenv);




var instance = new Lab03.AgentFunctions();
var gpt4o = LLMConfiguration.GetAzureOpenAIGPT4o();

// AutoGen makes use of AutoGen.SourceGenerator to automatically generate FunctionDefinition and FunctionCallWrapper for you.
// The FunctionDefinition will be created based on function signature and XML documentation.
// The return type of type-safe function needs to be Task<string>. And to get the best performance, please try only use primitive types and arrays of primitive types as parameters.
var config = new ConversableAgentConfig
{
    Temperature = 0,
    ConfigList = [gpt4o],
    FunctionContracts = new[]
    {
        instance.ConcatStringFunctionContract,
        instance.UpperCaseFunctionContract,
        instance.CalculateTaxFunctionContract,
    },
};

var agent = new AssistantAgent(
    name: "agent",
    systemMessage: "You are a helpful AI assistant",
    llmConfig: config,
    functionMap: new Dictionary<string, Func<string, Task<string>>>
    {
        { nameof(ConcatString), instance.ConcatStringWrapper },
        { nameof(UpperCase), instance.UpperCaseWrapper },
        { nameof(CalculateTax), instance.CalculateTaxWrapper },
    })
    .RegisterPrintMessage();

// talk to the assistant agent
var upperCase = await agent.SendAsync("convert to upper case: hello world");
upperCase.GetContent()?.Should().Be("HELLO WORLD");
upperCase.Should().BeOfType<AggregateMessage<ToolCallMessage, ToolCallResultMessage>>();
upperCase.GetToolCalls().Should().HaveCount(1);
upperCase.GetToolCalls().First().FunctionName.Should().Be(nameof(UpperCase));

var concatString = await agent.SendAsync("concatenate strings: a, b, c, d, e");
concatString.GetContent()?.Should().Be("a b c d e");
concatString.Should().BeOfType<AggregateMessage<ToolCallMessage, ToolCallResultMessage>>();
concatString.GetToolCalls().Should().HaveCount(1);
concatString.GetToolCalls().First().FunctionName.Should().Be(nameof(ConcatString));

var calculateTax = await agent.SendAsync("calculate tax: 100, 0.1");
calculateTax.GetContent().Should().Be("tax is 10");
calculateTax.Should().BeOfType<AggregateMessage<ToolCallMessage, ToolCallResultMessage>>();
calculateTax.GetToolCalls().Should().HaveCount(1);
calculateTax.GetToolCalls().First().FunctionName.Should().Be(nameof(CalculateTax));