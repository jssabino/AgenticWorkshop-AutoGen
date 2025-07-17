# Lab 05: AutoGen and MCP Integration

## 🎯 Objectives
- Understand the Model Context Protocol (MCP)
- Integrate AutoGen with external tools via MCP
- Connect AutoGen agents to GitHub MCP server
- Build sophisticated tool-enabled agent workflows
- Configure MCP integration in both code and AutoGen Studio

## 📋 Prerequisites
- Completed Labs 01-03
- Understanding of AutoGen multi-agent systems
- GitHub account and personal access token
- Node.js 18+ and npm (for MCP servers)
- OpenAI API key or Azure OpenAI access

## 🚀 Getting Started

### Step 1: Understanding MCP
The Model Context Protocol (MCP) is an open standard for connecting AI systems with external tools and data sources. It enables:
- Secure tool integration
- Standardized communication protocols
- Extensible architecture for custom tools
- Real-time data access for AI agents

### Step 2: Lab Structure
This lab is divided into two main sections:
- **Code Integration**: Programmatic MCP integration with AutoGen
- **Studio Integration**: Visual MCP configuration in AutoGen Studio

## 🔍 What You'll Learn

### 1. MCP Architecture
- **MCP Servers**: External tools and services
- **MCP Clients**: AI systems that consume MCP services
- **Protocol**: Standardized communication format
- **Security**: Authentication and authorization mechanisms

### 2. AutoGen + MCP Integration
- **Tool Registration**: Adding MCP tools to AutoGen agents
- **Function Calling**: Invoking MCP tools from conversations
- **Data Flow**: Managing data between agents and external services
- **Error Handling**: Robust error management in integrated systems

### 3. GitHub MCP Server
- **Repository Operations**: Clone, read, write, commit operations
- **Issue Management**: Creating, updating, and managing GitHub issues
- **Pull Request Workflows**: Automating PR creation and management
- **Code Analysis**: AI-powered code review and suggestions

## 📚 MCP Deep Dive

### MCP Protocol Overview
MCP defines a standard way for AI systems to:
1. **Discovery**: Find available tools and their capabilities
2. **Authentication**: Securely connect to external services
3. **Invocation**: Execute tool functions with parameters
4. **Response**: Receive and process tool results

### MCP Message Types
- **Initialize**: Establish connection and capabilities
- **List Tools**: Discover available tools
- **Call Tool**: Execute a specific tool function
- **Get Resource**: Retrieve data from external sources
- **Subscribe**: Monitor real-time updates

### Security Model
- **Authentication**: API keys, OAuth, or other auth mechanisms
- **Authorization**: Permission-based access control
- **Encryption**: Secure data transmission
- **Sandboxing**: Isolated execution environments

## 🛠 Code Integration Section

### Setting Up MCP with AutoGen

#### 1. Install MCP Dependencies
```bash
npm install @modelcontextprotocol/sdk
npm install @modelcontextprotocol/server-github
```

#### 2. Create MCP Client Configuration
```json
{
  "mcp_servers": {
    "github": {
      "command": "npx",
      "args": ["@modelcontextprotocol/server-github"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "your_github_token"
      }
    }
  }
}
```

#### 3. AutoGen Agent with MCP Tools
```csharp
public class MCPEnabledAgent : AssistantAgent
{
    private readonly MCPClient _mcpClient;
    
    public MCPEnabledAgent(string name, string systemMessage, 
                          LLMConfig llmConfig, MCPClient mcpClient)
        : base(name, systemMessage, llmConfig)
    {
        _mcpClient = mcpClient;
        RegisterMCPTools();
    }
    
    private void RegisterMCPTools()
    {
        // Register GitHub MCP tools
        RegisterTool("create_github_issue", CreateGitHubIssue);
        RegisterTool("search_github_repos", SearchGitHubRepos);
        RegisterTool("read_file_content", ReadFileContent);
    }
    
    private async Task<string> CreateGitHubIssue(string title, string body, string repo)
    {
        var result = await _mcpClient.CallTool("create_issue", new
        {
            title = title,
            body = body,
            repository = repo
        });
        return result.ToString();
    }
}
```

## 🎮 Exercises

### Exercise 1: Basic MCP Connection
1. Set up a GitHub MCP server
2. Create an AutoGen agent with MCP capabilities
3. Test basic GitHub operations (list repos, read files)

### Exercise 2: AI-Powered Code Review
1. Create a code review agent with GitHub MCP access
2. Implement file reading and analysis capabilities
3. Generate automated code review comments

### Exercise 3: Issue Management Workflow
1. Build a multi-agent system for GitHub issue management
2. Create agents for:
   - Issue triage and labeling
   - Assignment to team members
   - Progress tracking and updates

### Exercise 4: Automated PR Workflow
1. Create agents that can:
   - Analyze code changes
   - Generate PR descriptions
   - Suggest reviewers
   - Track PR status

## 🔧 Advanced Integration Patterns

### 1. Multi-Tool Agent
```csharp
public class MultiToolAgent : AssistantAgent
{
    public MultiToolAgent() : base("MultiTool", systemMessage, llmConfig)
    {
        // Register multiple MCP servers
        RegisterMCPServer("github", githubMCPClient);
        RegisterMCPServer("slack", slackMCPClient);
        RegisterMCPServer("jira", jiraMCPClient);
    }
    
    public async Task<string> CrossPlatformOperation(string operation, object parameters)
    {
        // Coordinate operations across multiple platforms
        var results = new List<string>();
        
        // Create GitHub issue
        var githubResult = await CallMCPTool("github", "create_issue", parameters);
        results.Add(githubResult);
        
        // Notify team on Slack
        var slackResult = await CallMCPTool("slack", "send_message", parameters);
        results.Add(slackResult);
        
        // Create JIRA ticket
        var jiraResult = await CallMCPTool("jira", "create_ticket", parameters);
        results.Add(jiraResult);
        
        return string.Join("\n", results);
    }
}
```

### 2. Workflow Orchestration
```csharp
public class WorkflowOrchestrator
{
    private readonly List<MCPEnabledAgent> _agents;
    private readonly Dictionary<string, MCPClient> _mcpClients;
    
    public async Task<WorkflowResult> ExecuteWorkflow(WorkflowDefinition workflow)
    {
        foreach (var step in workflow.Steps)
        {
            var agent = _agents.Find(a => a.Name == step.AgentName);
            var mcpClient = _mcpClients[step.MCPServer];
            
            var result = await agent.ExecuteWithMCP(step.Action, step.Parameters, mcpClient);
            
            if (!result.Success)
            {
                return new WorkflowResult { Success = false, Error = result.Error };
            }
        }
        
        return new WorkflowResult { Success = true };
    }
}
```

## 🏆 Best Practices

### 1. Error Handling
```csharp
public async Task<MCPResult> SafeMCPCall(string tool, object parameters)
{
    try
    {
        var result = await _mcpClient.CallTool(tool, parameters);
        return new MCPResult { Success = true, Data = result };
    }
    catch (MCPException ex)
    {
        _logger.LogError(ex, "MCP call failed: {Tool}", tool);
        return new MCPResult { Success = false, Error = ex.Message };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in MCP call: {Tool}", tool);
        return new MCPResult { Success = false, Error = "Unexpected error occurred" };
    }
}
```

### 2. Rate Limiting
```csharp
public class RateLimitedMCPClient
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Dictionary<string, DateTime> _lastCalls;
    
    public async Task<string> CallTool(string tool, object parameters)
    {
        await _semaphore.WaitAsync();
        try
        {
            await EnforceRateLimit(tool);
            return await _mcpClient.CallTool(tool, parameters);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private async Task EnforceRateLimit(string tool)
    {
        if (_lastCalls.ContainsKey(tool))
        {
            var timeSinceLastCall = DateTime.UtcNow - _lastCalls[tool];
            if (timeSinceLastCall < TimeSpan.FromSeconds(1))
            {
                await Task.Delay(TimeSpan.FromSeconds(1) - timeSinceLastCall);
            }
        }
        _lastCalls[tool] = DateTime.UtcNow;
    }
}
```

### 3. Security Considerations
- **API Key Management**: Store keys securely, rotate regularly
- **Permission Scoping**: Use minimal required permissions
- **Input Validation**: Validate all MCP tool parameters
- **Audit Logging**: Log all MCP operations for security monitoring

## 📖 Further Reading

### Official Documentation
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [MCP SDK Documentation](https://modelcontextprotocol.io/docs)
- [GitHub MCP Server](https://github.com/modelcontextprotocol/servers/tree/main/src/github)

### Implementation Guides
- [Building Custom MCP Servers](https://modelcontextprotocol.io/docs/building-servers)
- [MCP Client Implementation](https://modelcontextprotocol.io/docs/building-clients)
- [Security Best Practices](https://modelcontextprotocol.io/docs/security)

### Community Resources
- [MCP Examples Repository](https://github.com/modelcontextprotocol/examples)
- [MCP Discord Community](https://discord.gg/modelcontextprotocol)
- [AutoGen + MCP Integration Examples](https://github.com/microsoft/autogen/tree/main/samples/mcp)

## 🏆 Lab Completion

You've successfully completed Lab 04 when you can:
- [x] Understand MCP architecture and protocols
- [x] Set up and configure MCP servers
- [x] Integrate MCP tools with AutoGen agents
- [x] Build complex workflows with external tool integration
- [x] Handle errors and security in MCP integrations
- [x] Configure MCP in both code and AutoGen Studio

## 🎉 Workshop Completion

Congratulations! You've completed the entire AutoGen workshop. You now have:
- **Foundation**: Understanding of AutoGen basics and agent creation
- **Multi-Agent Systems**: Skills to build complex agent interactions
- **Visual Development**: Experience with AutoGen Studio
- **External Integration**: Ability to connect agents with external tools via MCP

## 🚀 Next Steps

Continue your AutoGen journey by:
1. Building custom agents for your specific use cases
2. Exploring advanced AutoGen features and patterns
3. Contributing to the AutoGen community
4. Integrating AutoGen into your production systems

---

**Estimated Time**: 75 minutes
**Difficulty**: Advanced
**Prerequisites**: Completed Labs 01-03, GitHub account, Node.js 18+
