# Lab 01: Introduction to AutoGen

## 🎯 Objectives
- Understand the core concepts of AutoGen
- Create your first AI agent
- Learn conversation patterns with message history
- Explore multi-agent conversations
- Implement function calling capabilities

## 📋 Prerequisites
- .NET 9.0 SDK installed
- Azure OpenAI access (configured for gpt-4o model)
- Basic C# knowledge

## 🚀 Getting Started

This lab contains three progressive examples that build upon each other:

### Step 1: Project Setup
Each lab example has its own project directory. Navigate to any of them:
```bash
cd src/lab01/lab01_1  # Basic agent
cd src/lab01/lab01_2  # Two-agent conversation
cd src/lab01/lab01_3  # Function calling agent
```

### Step 2: Configure API Keys
Copy the `.env.example` file to `.env` in each lab directory and configure your Azure OpenAI credentials:
```bash
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your_azure_openai_key_here
DEFAULT_MODEL=gpt-4o
```

### Step 3: Run the Examples
```bash
dotnet restore
dotnet run
```

## 🔍 What You'll Learn

### Lab 01_1: Basic Agent with Message History
In this first example, you'll create a simple AutoGen agent that:
- Responds to user messages with text transformation
- Maintains conversation history across multiple interactions
- Demonstrates basic agent configuration with Azure OpenAI

### Lab 01_2: Multi-Agent Conversation
Learn how to set up conversations between multiple agents:
- Teacher agent that creates math questions
- Student agent that answers questions
- Automatic conversation termination when correct answers are provided
- Message flow control and turn-taking

### Lab 01_3: Function Calling Agent
Explore advanced capabilities with function calling:
- Agent that can execute custom functions
- Multiple function definitions (uppercase, concatenation, calculations)
- Type-safe function contracts using AutoGen.SourceGenerator
- Function call validation and responses

## 📚 Code Walkthrough

### Lab 01_1: Basic Agent Creation
```csharp
// Load environment variables
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
Env.Load(dotenv);

// Configure Azure OpenAI
var gpt4o = LLMConfiguration.GetAzureOpenAIGPT4o();
var config = new ConversableAgentConfig
{
    Temperature = 0,
    ConfigList = [gpt4o],
};

// Create assistant agent
var assistantAgent = new AssistantAgent(
    name: "assistant",
    systemMessage: "You convert what user said to all uppercase.",
    llmConfig: config)
    .RegisterPrintMessage();

// Simple conversation
var reply = await assistantAgent.SendAsync("hello world");
// Result: "HELLO WORLD"
```

### Lab 01_2: Multi-Agent Conversation
```csharp
// Create teacher agent
var teacher = new AssistantAgent(
    name: "teacher",
    systemMessage: @"You are a teacher that create pre-school math question for student and check answer.
If the answer is correct, you terminate conversation by saying [TERMINATE].
If the answer is wrong, you ask student to fix it.",
    llmConfig: new ConversableAgentConfig
    {
        Temperature = 0,
        ConfigList = [gpt4o],
    })
    .RegisterPostProcess(async (_, reply, _) =>
    {
        if (reply.GetContent()?.ToLower().Contains("terminate") is true)
        {
            return new TextMessage(Role.Assistant, GroupChatExtension.TERMINATE, from: reply.From);
        }
        return reply;
    })
    .RegisterPrintMessage();

// Create student agent
var student = new AssistantAgent(
    name: "student",
    systemMessage: "You are a student that answer question from teacher",
    llmConfig: new ConversableAgentConfig
    {
        Temperature = 0,
        ConfigList = [gpt4o],
    })
    .RegisterPrintMessage();

// Start the conversation
var conversation = await student.InitiateChatAsync(
    receiver: teacher,
    message: "Hey teacher, please create math question for me.",
    maxRound: 10);
```

### Lab 01_3: Function Calling Setup
```csharp
// Define function class
public partial class AgentFunctions
{
    /// <summary>
    /// upper case the message when asked.
    /// </summary>
    [Function]
    public async Task<string> UpperCase(string message)
    {
        return message.ToUpper();
    }

    /// <summary>
    /// Concatenate strings.
    /// </summary>
    [Function]
    public async Task<string> ConcatString(string[] strings)
    {
        return string.Join(" ", strings);
    }

    /// <summary>
    /// calculate tax
    /// </summary>
    [Function]
    public async Task<string> CalculateTax(int price, float taxRate)
    {
        return $"tax is {price * taxRate}";
    }
}

// Configure agent with functions
var instance = new AgentFunctions();
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
        { nameof(AgentFunctions.ConcatString), instance.ConcatStringWrapper },
        { nameof(AgentFunctions.UpperCase), instance.UpperCaseWrapper },
        { nameof(AgentFunctions.CalculateTax), instance.CalculateTaxWrapper },
    })
    .RegisterPrintMessage();
```

## 🔧 Deep Dive: AutoGen Architecture

### Core Components

#### 1. Agents
- **AssistantAgent**: LLM-powered agent for general tasks and conversations
- **ConversableAgent**: Base class providing conversation capabilities
- **Message Processing**: Agents can register post-processing functions for custom behavior

#### 2. Message System
- **TextMessage**: Simple text-based messages with role information
- **ToolCallAggregateMessage**: Messages containing function call results
- **Conversation History**: Automatic tracking of message exchanges between agents

#### 3. LLM Configuration
- **ConversableAgentConfig**: Main configuration class for agent behavior
- **AzureOpenAIConfig**: Specific configuration for Azure OpenAI models
- **Function Contracts**: Type-safe function definitions for agent capabilities

### Configuration Management
AutoGen uses environment variables for secure configuration:
```csharp
internal static class LLMConfiguration
{
    public static AzureOpenAIConfig GetAzureOpenAIGPT4o(string deployName = "gpt-4o")
    {
        var azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") 
            ?? throw new Exception("Please set AZURE_OPENAI_API_KEY environment variable.");
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") 
            ?? throw new Exception("Please set AZURE_OPENAI_ENDPOINT environment variable.");

        return new AzureOpenAIConfig(endpoint, deployName, azureOpenAIKey);
    }
}
```

### Message Flow Patterns

#### 1. Single Agent Interaction
- User sends message to agent
- Agent processes with LLM
- Agent returns response
- History maintained for context

#### 2. Multi-Agent Conversation
- Initial agent sends message to receiver
- Turn-taking managed automatically
- Conversation continues until termination condition
- Full conversation history preserved

#### 3. Function Calling Flow
- Agent receives request requiring function execution
- Agent identifies appropriate function from contracts
- Function executed through registered wrapper
- Result incorporated into agent response

## 🛠 Exercises

### Exercise 1: Basic Agent Modification (lab01_1)
Modify the basic agent to perform different text transformations:
- Create an agent that reverses text instead of converting to uppercase
- Add temperature variation to see different response styles
- Experiment with conversation history length

### Exercise 2: Enhanced Multi-Agent System (lab01_2)
Extend the teacher-student conversation:
- Add a third agent that acts as a "grader" to evaluate answers
- Create different subject areas (history, science, language)
- Implement different difficulty levels

### Exercise 3: Advanced Function Calling (lab01_3)
Add new functions to the agent capabilities:
- Mathematical operations (add, subtract, multiply, divide)
- String manipulation functions (reverse, remove spaces, count words)
- Date/time functions (current time, format dates)

### Exercise 4: Error Handling and Validation
Implement robust error handling across all examples:
- API connection failures
- Invalid function parameters
- Conversation timeout scenarios

## 🧪 Advanced Topics

### 1. Custom Message Processing
Implement custom post-processing for agent responses:

```csharp
var agent = new AssistantAgent(name: "agent", systemMessage: "...", llmConfig: config)
    .RegisterPostProcess(async (_, reply, _) =>
    {
        // Custom processing logic
        if (reply.GetContent()?.Contains("special_keyword") is true)
        {
            return new TextMessage(Role.Assistant, "Special response triggered!", from: reply.From);
        }
        return reply;
    });
```

### 2. Function Contract Generation
AutoGen uses SourceGenerator to automatically create function contracts:

```csharp
// The [Function] attribute automatically generates:
// - FunctionDefinition based on method signature
// - FunctionCallWrapper for type-safe execution
// - XML documentation becomes function description

[Function]
public async Task<string> MyFunction(string input, int count)
{
    // Implementation
    return result;
}
```

### 3. Conversation Termination Patterns
Control conversation flow with termination conditions:

```csharp
// Keyword-based termination
.RegisterPostProcess(async (_, reply, _) =>
{
    if (reply.GetContent()?.ToLower().Contains("terminate") is true)
    {
        return new TextMessage(Role.Assistant, GroupChatExtension.TERMINATE, from: reply.From);
    }
    return reply;
});

// Round-based termination
var conversation = await agent1.InitiateChatAsync(
    receiver: agent2,
    message: "Start conversation",
    maxRound: 5); // Automatically stops after 5 rounds
```

### 4. Environment Configuration Best Practices
Secure and flexible configuration management:

```csharp
// Use DotNetEnv for .env file loading
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
Env.Load(dotenv);

// Centralized configuration class
internal static class LLMConfiguration
{
    public static AzureOpenAIConfig GetAzureOpenAIGPT4o(string deployName = "gpt-4o")
    {
        var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") 
            ?? throw new Exception("Missing API key");
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") 
            ?? throw new Exception("Missing endpoint");
        
        return new AzureOpenAIConfig(endpoint, deployName, key);
    }
}
```

## 🔍 Troubleshooting

### Common Issues

#### 1. API Configuration Issues
- **Azure OpenAI Endpoint**: Ensure your endpoint URL is correctly formatted
- **API Key**: Verify your Azure OpenAI API key has proper permissions
- **Model Deployment**: Confirm the gpt-4o model is deployed in your Azure OpenAI resource
- **Environment Variables**: Check that .env file is properly loaded

#### 2. Function Calling Issues
- **Function Contracts**: Ensure all functions are properly decorated with `[Function]` attribute
- **Return Types**: Function return type must be `Task<string>` for compatibility
- **Parameter Types**: Use primitive types and arrays for best performance
- **Function Registration**: Verify function contracts and wrappers are properly registered

#### 3. Conversation Flow Issues
- **Message Types**: Check that agents return expected message types (`TextMessage`, `ToolCallAggregateMessage`)
- **Termination**: Ensure termination conditions are properly implemented
- **History Management**: Verify conversation history is maintained correctly across turns

#### 4. Build and Runtime Issues
- **.NET Version**: Ensure .NET 9.0 SDK is installed and being used
- **Package Versions**: Verify AutoGen package versions are compatible (all 0.2.3)
- **DotNetEnv**: Confirm .env files are copied to output directory during build

## 📖 Further Reading

### Official Documentation
- [AutoGen Agent Types](https://microsoft.github.io/autogen/docs/topics/non-openai-models/about-using-nonopenai-models)
- [Message Handling](https://microsoft.github.io/autogen/docs/topics/handling-complex-outputs)
- [LLM Configuration](https://microsoft.github.io/autogen/docs/topics/llm_configuration)

### Research Papers
- [Conversational AI with AutoGen](https://arxiv.org/abs/2308.08155)
- [Multi-Agent Communication Patterns](https://www.microsoft.com/en-us/research/publication/autogen-enabling-next-gen-llm-applications-via-multi-agent-conversation/)

### Community Resources
- [AutoGen Examples](https://github.com/microsoft/autogen/tree/main/samples)
- [Best Practices Guide](https://microsoft.github.io/autogen/docs/Getting-Started/)
- [Common Patterns](https://microsoft.github.io/autogen/docs/topics/conversation-patterns)

## 🏆 Lab Completion

You've successfully completed Lab 01 when you can:
- [x] Create and configure a basic AutoGen agent (lab01_1)
- [x] Implement conversation history management
- [x] Set up multi-agent conversations with termination (lab01_2)
- [x] Understand message flow between agents
- [x] Implement function calling capabilities (lab01_3)
- [x] Configure Azure OpenAI integration
- [x] Handle environment variable configuration

### Key Takeaways
- **Agent Configuration**: Understanding how to set up agents with proper LLM configurations
- **Message Types**: Different message types and their purposes in conversations
- **Function Integration**: How to extend agent capabilities with custom functions
- **Conversation Patterns**: Single-agent interactions vs. multi-agent conversations
- **Environment Setup**: Secure configuration management with environment variables

## ➡️ Next Steps

Ready for more complex scenarios? Continue to [Lab 02: Multi-Agent Systems](../lab02/README.md) to learn how to create systems with multiple interacting agents.

---

**Estimated Time**: 60 minutes
**Difficulty**: Beginner
**Prerequisites**: Basic C# knowledge, Azure OpenAI access
**Key Concepts**: Agent creation, conversation flow, function calling, message processing
