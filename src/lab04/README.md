# Lab 04: AutoGen Studio

## 🎯 Objectives
- Set up and configure AutoGen Studio
- Create multi-agent workflows using a visual interface
- Understand the no-code approach to agent development
- Build and deploy agent conversations through the Studio UI

## 📋 Prerequisites
- Completed Lab 01 and Lab 02
- Python 3.8+ installed
- OpenAI API key or Azure OpenAI access
- Basic understanding of multi-agent systems

## 🚀 Getting Started

### Step 1: Install AutoGen Studio
```bash
pip install autogenstudio
```

### Step 2: Launch AutoGen Studio
```bash
autogenstudio ui --port 8081
```

### Step 3: Open in Browser
Navigate to `http://localhost:8081` in your web browser.

## 🔍 What You'll Learn

### 1. AutoGen Studio Interface
- **Agent Designer**: Visual agent creation and configuration
- **Workflow Builder**: Drag-and-drop conversation flow design
- **Chat Interface**: Real-time conversation testing
- **Configuration Manager**: Model and API settings

### 2. Visual Agent Development
- **Agent Templates**: Pre-built agent configurations
- **Custom Agents**: Creating specialized agents visually
- **Agent Relationships**: Defining interaction patterns
- **Conversation Flows**: Designing multi-agent workflows

### 3. No-Code Multi-Agent Systems
- **Workflow Templates**: Common multi-agent patterns
- **Event-Driven Flows**: Reactive agent behaviors
- **Conditional Logic**: Decision-making in conversations
- **Integration Points**: Connecting with external services

## 📚 Studio Components Deep Dive

### Agent Designer
The Agent Designer allows you to create and configure agents without writing code:

#### Agent Configuration
- **Name**: Unique identifier for the agent
- **Role**: System message defining agent behavior
- **Model**: LLM model selection (GPT-4, GPT-3.5, etc.)
- **Parameters**: Temperature, max tokens, etc.
- **Skills**: Available functions and tools

#### Agent Types
- **Assistant**: General-purpose conversational agent
- **UserProxy**: Represents human users
- **GroupChatManager**: Orchestrates multi-agent conversations
- **Custom**: Specialized agents with specific roles

### Workflow Builder
Design complex multi-agent workflows visually:

#### Flow Elements
- **Start Node**: Conversation initiation
- **Agent Nodes**: Individual agent interactions
- **Decision Nodes**: Conditional branching
- **End Node**: Conversation termination

#### Connection Types
- **Sequential**: One agent after another
- **Parallel**: Multiple agents simultaneously
- **Conditional**: Based on conversation context
- **Loop**: Repeating interactions

### Chat Interface
Test and refine your multi-agent systems:

#### Features
- **Real-time Chat**: Live conversation testing
- **Agent Switching**: Manual agent control
- **Message History**: Conversation tracking
- **Export Options**: Save conversations for analysis

## 🛠 Practical Examples

### Example 1: Customer Service Team
Create a customer service team using the Studio interface:

1. **Create Agents**:
   - Intake Agent (receives customer requests)
   - Technical Support (handles technical issues)
   - Billing Support (handles billing inquiries)
   - Manager (escalation handling)

2. **Build a Team**:
   - Customer → Intake Agent
   - Intake Agent → Appropriate Specialist
   - Specialist → Manager (if escalation needed)

3. **Configure Rules**:
   - Route technical issues to Technical Support
   - Route billing questions to Billing Support
   - Escalate unresolved issues to Manager

### Example 2: Content Creation Team
Build a content creation workflow:

1. **Create Agents**:
   - Content Strategist (plans content)
   - Writer (creates content)
   - Editor (reviews and refines)
   - Publisher (finalizes and publishes)

2. **Design Workflow**:
   - Strategist → Writer → Editor → Publisher
   - Editor → Writer (for revisions)
   - Publisher → Strategist (for feedback)

## 🔧 Configuration Files

### Agent Configuration
```json
{
  "name": "TechnicalSupport",
  "type": "assistant",
  "config": {
    "model": "gpt-4",
    "temperature": 0.7,
    "max_tokens": 1000,
    "system_message": "You are a technical support specialist...",
    "functions": [
      {
        "name": "search_knowledge_base",
        "description": "Search the technical knowledge base"
      }
    ]
  }
}
```

### Workflow Configuration
```json
{
  "name": "CustomerServiceWorkflow",
  "description": "Multi-agent customer service workflow",
  "agents": [
    {
      "id": "intake",
      "config_path": "./agents/intake_agent.json"
    },
    {
      "id": "technical",
      "config_path": "./agents/technical_support.json"
    },
    {
      "id": "billing",
      "config_path": "./agents/billing_support.json"
    }
  ],
  "workflow": {
    "start": "intake",
    "flows": [
      {
        "from": "intake",
        "to": "technical",
        "condition": "technical_issue"
      },
      {
        "from": "intake",
        "to": "billing",
        "condition": "billing_issue"
      }
    ]
  }
}
```

### Settings Configuration
```json
{
  "models": {
    "openai": {
      "api_key": "your_openai_api_key",
      "api_base": "https://api.openai.com/v1",
      "api_version": "2024-02-01"
    },
    "azure_openai": {
      "api_key": "your_azure_openai_key",
      "api_base": "https://your-resource.openai.azure.com/",
      "api_version": "2024-02-01"
    }
  },
  "default_model": "gpt-4",
  "max_tokens": 2000,
  "temperature": 0.7
}
```

## 🧪 Advanced Features

### 1. Custom Functions
Add custom functions to agents:

```json
{
  "functions": [
    {
      "name": "get_weather",
      "description": "Get current weather information",
      "parameters": {
        "type": "object",
        "properties": {
          "location": {
            "type": "string",
            "description": "The city and state, e.g. San Francisco, CA"
          }
        },
        "required": ["location"]
      }
    }
  ]
}
```

### 2. Conditional Logic
Implement decision-making in workflows:

```json
{
  "decision_nodes": [
    {
      "id": "issue_classifier",
      "condition": "message_contains('technical')",
      "true_path": "technical_support",
      "false_path": "general_support"
    }
  ]
}
```

### 3. Integration with External APIs
Connect agents to external services:

```json
{
  "integrations": [
    {
      "name": "knowledge_base",
      "type": "rest_api",
      "url": "https://api.knowledge.com/search",
      "authentication": {
        "type": "bearer",
        "token": "your_api_token"
      }
    }
  ]
}
```

## 🎮 Hands-On Exercises

### Exercise 1: Basic Agent Creation
1. Open AutoGen Studio
2. Create a simple assistant agent
3. Configure it with a custom system message
4. Test it in the chat interface

### Exercise 2: Multi-Agent Workflow
1. Create three agents: Planner, Executor, Reviewer
2. Design a workflow where:
   - Planner creates a plan
   - Executor implements the plan
   - Reviewer provides feedback
3. Test the complete workflow

### Exercise 3: Customer Service Simulator
1. Create a customer service team with 4 agents
2. Design routing logic based on inquiry type
3. Add escalation paths for complex issues
4. Test with different customer scenarios

### Exercise 4: Content Creation Pipeline
1. Create a content creation team
2. Design a workflow for blog post creation
3. Add revision loops and approval gates
4. Test with a sample content request

## 🔍 Troubleshooting

### Common Issues

#### 1. Agent Not Responding
- Check API key configuration
- Verify model availability
- Review system message formatting
- Check network connectivity

#### 2. Workflow Stuck in Loop
- Review conditional logic
- Add termination conditions
- Check agent response patterns
- Implement timeout mechanisms

#### 3. Performance Issues
- Optimize agent system messages
- Reduce conversation context length
- Use appropriate model for task complexity
- Implement caching where possible

#### 4. Configuration Errors
- Validate JSON syntax
- Check required fields
- Verify file paths
- Review agent references

## 🏆 Best Practices

### 1. Agent Design
- **Clear Roles**: Define specific, focused roles for each agent
- **Consistent Naming**: Use descriptive, consistent agent names
- **Appropriate Models**: Choose models based on task complexity
- **System Messages**: Write clear, specific system messages

### 2. Workflow Design
- **Logical Flow**: Design intuitive conversation flows
- **Error Handling**: Include error recovery paths
- **Termination Conditions**: Define clear end points
- **Testing**: Thoroughly test all workflow paths

### 3. Configuration Management
- **Version Control**: Track configuration changes
- **Documentation**: Document agent roles and workflows
- **Backup**: Maintain configuration backups
- **Security**: Secure API keys and sensitive data

### 4. Performance Optimization
- **Conversation Length**: Monitor and manage context length
- **Model Selection**: Use appropriate models for each task
- **Caching**: Implement response caching where beneficial
- **Load Testing**: Test workflows under load

## 📁 Studio Files Structure

```
lab03/
├── agents/
│   ├── intake_agent.json
│   ├── technical_support.json
│   ├── billing_support.json
│   └── manager.json
├── workflows/
│   ├── customer_service.json
│   ├── content_creation.json
│   └── research_team.json
├── settings/
│   ├── models.json
│   ├── api_keys.json
│   └── preferences.json
└── exports/
    ├── conversations/
    └── reports/
```

## 📖 Further Reading

### Official Documentation
- [AutoGen Studio Documentation](https://microsoft.github.io/autogen/docs/ecosystem/autogen-studio)
- [AutoGen Studio GitHub](https://github.com/microsoft/autogen/tree/main/samples/apps/autogen-studio)
- [Studio Configuration Guide](https://microsoft.github.io/autogen/docs/ecosystem/autogen-studio/configuration)

### Video Tutorials
- [AutoGen Studio Getting Started](https://www.youtube.com/watch?v=your_video_id)
- [Building Multi-Agent Workflows](https://www.youtube.com/watch?v=your_video_id)
- [Advanced Studio Features](https://www.youtube.com/watch?v=your_video_id)

### Community Resources
- [AutoGen Studio Examples](https://github.com/microsoft/autogen/tree/main/samples/apps/autogen-studio/examples)
- [Community Workflows](https://github.com/microsoft/autogen/discussions)
- [Studio Tips and Tricks](https://microsoft.github.io/autogen/docs/ecosystem/autogen-studio/tips)

## 🏆 Lab Completion

You've successfully completed Lab 03 when you can:
- [x] Set up and configure AutoGen Studio
- [x] Create agents using the visual interface
- [x] Design multi-agent workflows
- [x] Test conversations in real-time
- [x] Export and manage configurations
- [x] Troubleshoot common issues

## ➡️ Next Steps

Ready to integrate external tools and services? Continue to [Lab 04: AutoGen and MCP Integration](../lab04/README.md) to learn how to connect AutoGen with external systems using the Model Context Protocol.

---

**Estimated Time**: 45 minutes
**Difficulty**: Intermediate
**Prerequisites**: Completed Labs 01-02, Python 3.8+
