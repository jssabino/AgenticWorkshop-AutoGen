# Lab 02: Multi-Agent Systems

## 🎯 Objectives
- Build complex multi-agent conversational systems
- Understand agent orchestration and communication patterns
- Learn group chat management and agent coordination
- Implement specialized agent roles and responsibilities

## 📋 Prerequisites
- Completed Lab 01
- Understanding of AutoGen agent basics
- OpenAI API key or Azure OpenAI access

## 🚀 Getting Started

### Step 1: Project Setup
Navigate to the lab02 directory and restore dependencies:
```bash
cd src/lab02
dotnet restore
```

### Step 2: Configure API Keys
Copy the .env.example file to .env and configure your API keys:
```bash
cp .env.example .env
# Edit .env with your API key
```

### Step 3: Run the Multi-Agent Demo
```bash
dotnet run
```

## 🔍 What You'll Learn

### 1. Multi-Agent Architecture
- **Agent Roles**: Different types of agents with specific responsibilities
- **Communication Patterns**: How agents interact and coordinate
- **Conversation Flow**: Managing turn-taking and message routing
- **Group Dynamics**: Orchestrating conversations with multiple participants

### 2. Agent Specialization
- **Code Writer**: Agent specialized in writing code
- **Code Reviewer**: Agent that reviews and improves code
- **Project Manager**: Agent that coordinates tasks and decisions
- **User Proxy**: Agent representing human users

### 3. Advanced Conversation Patterns
- **Sequential Conversations**: Agents taking turns in order
- **Broadcast Conversations**: One agent communicating with multiple agents
- **Hierarchical Conversations**: Manager-worker relationships
- **Collaborative Conversations**: Agents working together on tasks

## 📚 Code Walkthrough

### Multi-Agent System Setup
```csharp
// Create specialized agents
var codeWriter = new AssistantAgent(
    name: "CodeWriter",
    systemMessage: "You are an expert software developer...",
    llmConfig: openAIConfig);

var codeReviewer = new AssistantAgent(
    name: "CodeReviewer", 
    systemMessage: "You are a senior code reviewer...",
    llmConfig: openAIConfig);

var projectManager = new AssistantAgent(
    name: "ProjectManager",
    systemMessage: "You are a project manager...",
    llmConfig: openAIConfig);
```

### Group Chat Management
```csharp
var groupChat = new GroupChat(
    agents: new[] { codeWriter, codeReviewer, projectManager },
    messages: new List<IMessage>(),
    maxRound: 10);

var groupChatManager = new GroupChatManager(
    groupChat: groupChat,
    llmConfig: openAIConfig);
```

## 🔧 Deep Dive: Multi-Agent Communication

### Communication Patterns

#### 1. Sequential Pattern
Agents take turns in a predefined order:
```
User → Agent A → Agent B → Agent C → User
```

#### 2. Hub-and-Spoke Pattern
Central agent coordinates with multiple specialized agents:
```
       Agent B
         ↑
User → Manager → Agent C
         ↓
       Agent D
```

#### 3. Collaborative Pattern
Multiple agents work together on a shared task:
```
Agent A ↔ Agent B
   ↕       ↕
Agent C ↔ Agent D
```

### Message Routing
AutoGen provides several strategies for message routing:

#### 1. Round Robin
```csharp
var groupChat = new GroupChat(
    agents: agents,
    messages: messages,
    speakerSelectionMethod: SpeakerSelectionMethod.RoundRobin);
```

#### 2. Manual Selection
```csharp
var groupChat = new GroupChat(
    agents: agents,
    messages: messages,
    speakerSelectionMethod: SpeakerSelectionMethod.Manual);
```

#### 3. Auto Selection
```csharp
var groupChat = new GroupChat(
    agents: agents,
    messages: messages,
    speakerSelectionMethod: SpeakerSelectionMethod.Auto);
```

### Agent Coordination Mechanisms

#### 1. Shared Context
All agents have access to the conversation history:
```csharp
public class SharedContextAgent : AssistantAgent
{
    private readonly ConversationHistory _sharedHistory;
    
    public SharedContextAgent(ConversationHistory sharedHistory) 
    {
        _sharedHistory = sharedHistory;
    }
}
```

#### 2. State Management
Agents can maintain and share state:
```csharp
public class StatefulAgent : AssistantAgent
{
    private readonly Dictionary<string, object> _sharedState;
    
    public void UpdateState(string key, object value)
    {
        _sharedState[key] = value;
    }
}
```

#### 3. Event-Driven Communication
Agents can react to events from other agents:
```csharp
public class EventDrivenAgent : AssistantAgent
{
    public event EventHandler<AgentEventArgs> TaskCompleted;
    
    protected virtual void OnTaskCompleted(AgentEventArgs e)
    {
        TaskCompleted?.Invoke(this, e);
    }
}
```

## 🛠 Practical Examples

### Example 1: Code Development Team
A team of agents working together to develop software:

- **Product Owner**: Defines requirements
- **Developer**: Writes code
- **Code Reviewer**: Reviews and suggests improvements
- **Tester**: Creates test cases

### Example 2: Research Team
A team conducting research on a topic:

- **Research Lead**: Coordinates research direction
- **Data Analyst**: Analyzes data and statistics
- **Writer**: Synthesizes findings into reports
- **Critic**: Provides critical feedback

### Example 3: Customer Support Team
A team handling customer inquiries:

- **Intake Agent**: Receives and categorizes requests
- **Technical Expert**: Handles technical issues
- **Account Manager**: Manages account-related queries
- **Escalation Manager**: Handles complex cases

## 🧪 Advanced Topics

### 1. Dynamic Agent Creation
Create agents on-demand based on requirements:

```csharp
public class AgentFactory
{
    public IAgent CreateAgent(AgentType type, string specialization)
    {
        return type switch
        {
            AgentType.Developer => new DeveloperAgent(specialization),
            AgentType.Reviewer => new ReviewerAgent(specialization),
            AgentType.Manager => new ManagerAgent(specialization),
            _ => throw new ArgumentException($"Unknown agent type: {type}")
        };
    }
}
```

### 2. Agent Workflows
Define complex workflows with multiple stages:

```csharp
public class WorkflowOrchestrator
{
    public async Task<WorkflowResult> ExecuteWorkflow(WorkflowDefinition workflow)
    {
        foreach (var stage in workflow.Stages)
        {
            var stageResult = await ExecuteStage(stage);
            if (!stageResult.Success)
            {
                return new WorkflowResult { Success = false, Error = stageResult.Error };
            }
        }
        return new WorkflowResult { Success = true };
    }
}
```

### 3. Agent Performance Monitoring
Monitor agent performance and behavior:

```csharp
public class AgentMonitor
{
    public void MonitorAgent(IAgent agent)
    {
        agent.MessageSent += (sender, args) => LogMessage(args);
        agent.MessageReceived += (sender, args) => LogMessage(args);
        agent.ErrorOccurred += (sender, args) => LogError(args);
    }
}
```

## 🎮 Exercises

### Exercise 1: Basic Multi-Agent System
Create a simple system with three agents:
- One agent that asks questions
- One agent that provides answers
- One agent that evaluates the quality of answers

### Exercise 2: Specialized Team
Build a software development team with:
- Product Manager (defines requirements)
- Developer (writes code)
- QA Engineer (tests code)
- DevOps Engineer (handles deployment)

### Exercise 3: Debate System
Create a debate system where:
- Multiple agents argue different sides of an issue
- A moderator manages the debate
- A judge evaluates the arguments

### Exercise 4: Customer Service Simulation
Build a customer service system with:
- Customer (asks questions)
- Level 1 Support (handles basic issues)
- Level 2 Support (handles complex issues)
- Manager (handles escalations)

## 🔍 Troubleshooting

### Common Issues

#### 1. Agent Conflicts
When agents provide conflicting information:
- Implement conflict resolution mechanisms
- Use voting systems for decisions
- Designate authoritative agents for specific domains

#### 2. Infinite Loops
Prevent agents from talking indefinitely:
- Set maximum conversation rounds
- Implement termination conditions
- Use conversation moderators

#### 3. Performance Issues
Optimize multi-agent system performance:
- Implement efficient message routing
- Use caching for frequently accessed data
- Optimize LLM API calls

#### 4. Context Management
Handle large conversation contexts:
- Implement conversation summarization
- Use selective context retention
- Implement context compression techniques

## 🏆 Best Practices

### 1. Agent Design
- **Single Responsibility**: Each agent should have a clear, focused role
- **Clear Communication**: Agents should communicate clearly and concisely
- **Error Handling**: Implement robust error handling and recovery
- **State Management**: Maintain consistent state across agents

### 2. System Architecture
- **Scalability**: Design for scalability from the start
- **Modularity**: Keep agents loosely coupled
- **Monitoring**: Implement comprehensive logging and monitoring
- **Testing**: Create comprehensive test suites for multi-agent interactions

### 3. Performance Optimization
- **Batch Processing**: Process multiple messages in batches when possible
- **Caching**: Cache frequently used data and responses
- **Load Balancing**: Distribute work across multiple agent instances
- **Resource Management**: Monitor and manage computational resources

## 📖 Further Reading

### Official Documentation
- [AutoGen Group Chat](https://microsoft.github.io/autogen/docs/topics/groupchat/overview)
- [Multi-Agent Conversation](https://microsoft.github.io/autogen/docs/topics/conversation-patterns)
- [Agent Communication](https://microsoft.github.io/autogen/docs/topics/handling-complex-outputs)

### Research Papers
- [Multi-Agent Systems in AI](https://arxiv.org/abs/2308.08155)
- [Coordination in Multi-Agent Systems](https://www.microsoft.com/en-us/research/publication/autogen-enabling-next-gen-llm-applications-via-multi-agent-conversation/)

### Community Resources
- [Multi-Agent Examples](https://github.com/microsoft/autogen/tree/main/samples)
- [Best Practices for Multi-Agent Systems](https://microsoft.github.io/autogen/docs/Getting-Started/)
- [Advanced Multi-Agent Patterns](https://microsoft.github.io/autogen/docs/topics/conversation-patterns)

## 🏆 Lab Completion

You've successfully completed Lab 02 when you can:
- [x] Create multiple specialized agents
- [x] Orchestrate multi-agent conversations
- [x] Implement agent coordination mechanisms
- [x] Handle complex conversation flows
- [x] Monitor and debug multi-agent systems

## ➡️ Next Steps

Ready to explore visual agent development? Continue to [Lab 03: AutoGen Studio](../lab03/README.md) to learn how to create multi-agent systems using a visual interface.

---

**Estimated Time**: 60 minutes
**Difficulty**: Intermediate
**Prerequisites**: Completed Lab 01
