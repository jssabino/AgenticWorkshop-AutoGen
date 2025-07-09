# Lab 01: Introduction to AutoGen

## 🎯 Objectives
- Understand the core concepts of AutoGen
- Create your first AI agent
- Learn basic conversation patterns
- Explore agent configuration options

## 📋 Prerequisites
- .NET 8.0 SDK installed
- OpenAI API key or Azure OpenAI access
- Basic C# knowledge

## 🚀 Getting Started

### Step 1: Project Setup
Navigate to the lab01 directory and restore dependencies:
```bash
cd src/lab01
dotnet restore
```

### Step 2: Configure API Keys
Create a `.env` file in the lab01 directory:
```
OPENAI_API_KEY=your_openai_api_key_here
```

### Step 3: Run the Basic Example
```bash
dotnet run
```

## 🔍 What You'll Learn

### 1. AutoGen Agent Basics
In this lab, you'll create a simple AutoGen agent that can:
- Respond to user messages
- Maintain conversation context
- Execute basic reasoning tasks

### 2. Agent Configuration
Learn how to configure agents with:
- Custom system messages
- Model parameters (temperature, max tokens)
- Function calling capabilities
- Memory management

### 3. Conversation Flow
Understand how AutoGen handles:
- Message routing
- Turn-taking in conversations
- Context preservation
- Error handling

## 📚 Code Walkthrough

### Basic Agent Creation
```csharp
var agent = new AssistantAgent(
    name: "assistant",
    systemMessage: "You are a helpful AI assistant.",
    llmConfig: new OpenAIConfig
    {
        ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
        ModelId = "gpt-4"
    });
```

### Simple Conversation
```csharp
var userProxy = new UserProxyAgent(
    name: "user",
    humanInputMode: HumanInputMode.ALWAYS);

await userProxy.InitiateChatAsync(
    receiver: agent,
    message: "Hello, can you help me with a task?");
```

## 🔧 Deep Dive: AutoGen Architecture

### Core Components

#### 1. Agents
- **ConversableAgent**: Base class for all agents
- **AssistantAgent**: LLM-powered agent for general tasks
- **UserProxyAgent**: Represents human users in conversations
- **GroupChatManager**: Manages multi-agent conversations

#### 2. Message System
- **Message**: Base message class
- **TextMessage**: Simple text-based messages
- **FunctionCallMessage**: Messages with function calls
- **ImageMessage**: Messages with image content

#### 3. LLM Configuration
- **LLMConfig**: Base configuration for language models
- **OpenAIConfig**: Configuration for OpenAI models
- **AzureOpenAIConfig**: Configuration for Azure OpenAI

### Agent Lifecycle
1. **Initialization**: Agent is created with configuration
2. **Message Reception**: Agent receives messages from other agents
3. **Processing**: Agent processes the message using LLM
4. **Response Generation**: Agent generates appropriate response
5. **Message Sending**: Agent sends response to target recipient

### Memory Management
AutoGen agents maintain conversation history automatically:
- **Short-term Memory**: Recent conversation context
- **Long-term Memory**: Persistent storage across sessions
- **Selective Memory**: Filtering important information

## 🛠 Exercises

### Exercise 1: Basic Agent
Create a simple agent that introduces itself and asks for the user's name.

### Exercise 2: Specialized Agent
Create an agent with a specific role (e.g., "You are a helpful coding assistant specialized in C#").

### Exercise 3: Agent with Custom Parameters
Experiment with different model parameters:
- Temperature (0.0 - 1.0)
- Max tokens
- Top-p sampling

### Exercise 4: Error Handling
Add proper error handling for API failures and invalid responses.

## 🧪 Advanced Topics

### 1. Custom Agent Classes
Learn to create custom agent types by extending base classes:

```csharp
public class SpecializedAgent : AssistantAgent
{
    public SpecializedAgent(string name, string systemMessage, LLMConfig llmConfig) 
        : base(name, systemMessage, llmConfig)
    {
    }
    
    // Custom behavior methods
    public async Task<string> PerformSpecializedTask(string input)
    {
        // Custom logic here
        return await base.GenerateReplyAsync(new TextMessage(input));
    }
}
```

### 2. Function Calling
Enable agents to call external functions:

```csharp
var functionConfig = new FunctionConfig
{
    Functions = new List<FunctionDefinition>
    {
        new FunctionDefinition
        {
            Name = "get_weather",
            Description = "Get current weather information",
            Parameters = new { location = "string" }
        }
    }
};
```

### 3. Conversation Termination
Control when conversations should end:

```csharp
var terminationConfig = new TerminationConfig
{
    MaxRounds = 10,
    TerminationKeywords = new[] { "TERMINATE", "EXIT" }
};
```

## 🔍 Troubleshooting

### Common Issues

#### 1. API Key Issues
- Ensure your API key is correctly set in the environment
- Check API key permissions and quotas
- Verify the correct API endpoint

#### 2. Model Configuration
- Ensure model name is correct (e.g., "gpt-4", "gpt-3.5-turbo")
- Check model availability in your region
- Verify API version compatibility

#### 3. Memory Issues
- Monitor conversation length to avoid token limits
- Implement conversation truncation if needed
- Use conversation summarization for long chats

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
- [x] Create a basic AutoGen agent
- [x] Configure agent parameters
- [x] Handle simple conversations
- [x] Understand agent lifecycle
- [x] Implement error handling

## ➡️ Next Steps

Ready for more complex scenarios? Continue to [Lab 02: Multi-Agent Systems](../lab02/README.md) to learn how to create systems with multiple interacting agents.

---

**Estimated Time**: 45 minutes
**Difficulty**: Beginner
**Prerequisites**: Basic C# knowledge
