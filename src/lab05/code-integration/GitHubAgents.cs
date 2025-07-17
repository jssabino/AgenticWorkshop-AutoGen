using Microsoft.AutoGen.Abstractions;
using Microsoft.AutoGen.Agents;
using Microsoft.AutoGen.OpenAI;
using Microsoft.Extensions.Logging;

namespace Lab04CodeIntegration;

/// <summary>
/// Base class for AutoGen agents with MCP integration
/// </summary>
public abstract class MCPEnabledAgent
{
    protected readonly MCPClient _mcpClient;
    protected readonly string _openAiApiKey;
    protected readonly ILogger? _logger;
    
    public string Name { get; protected set; }
    public string SystemMessage { get; protected set; }
    
    protected MCPEnabledAgent(MCPClient mcpClient, string openAiApiKey, string name, string systemMessage, ILogger? logger = null)
    {
        _mcpClient = mcpClient;
        _openAiApiKey = openAiApiKey;
        Name = name;
        SystemMessage = systemMessage;
        _logger = logger;
    }
    
    /// <summary>
    /// Generate a response using the OpenAI API
    /// </summary>
    protected async Task<string> GenerateResponse(string prompt)
    {
        try
        {
            // Create OpenAI chat client
            var chatClient = new OpenAI.Chat.ChatClient("gpt-4", _openAiApiKey);
            
            // Create system message
            var systemMessage = OpenAI.Chat.ChatMessage.CreateSystemMessage(SystemMessage);
            var userMessage = OpenAI.Chat.ChatMessage.CreateUserMessage(prompt);
            
            // Generate response
            var response = await chatClient.CompleteChatAsync(new[] { systemMessage, userMessage });
            
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate response for agent {AgentName}", Name);
            return $"Error generating response: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Call an MCP tool safely with error handling
    /// </summary>
    protected async Task<string> CallMCPTool(string toolName, object parameters)
    {
        try
        {
            var result = await _mcpClient.CallTool(toolName, parameters);
            return result?.ToString() ?? "Tool executed successfully";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to call MCP tool {ToolName}", toolName);
            return $"Error calling tool {toolName}: {ex.Message}";
        }
    }
}

/// <summary>
/// GitHub repository explorer agent
/// </summary>
public class GitHubExplorerAgent : MCPEnabledAgent
{
    public GitHubExplorerAgent(MCPClient mcpClient, string openAiApiKey, ILogger? logger = null) 
        : base(mcpClient, openAiApiKey, "GitHubExplorer", 
               """
               You are a GitHub repository explorer agent. Your role is to analyze GitHub repositories and provide insights about:
               1. Repository structure and organization
               2. Code quality and patterns
               3. Activity levels and contribution patterns
               4. Technology stack and dependencies
               5. Documentation quality
               
               Use the available MCP tools to gather information about repositories and provide comprehensive analysis.
               Always be specific and provide actionable insights.
               """, logger)
    {
    }
    
    public async Task<string> AnalyzeRepositories(string username, string repositoriesData)
    {
        var prompt = $@"
        Analyze the following GitHub repositories for user/organization '{username}':
        
        Repository Data:
        {repositoriesData}
        
        Please provide:
        1. An overview of the repository portfolio
        2. Technology stack analysis
        3. Activity and contribution patterns
        4. Notable projects and their purposes
        5. Recommendations for improvement
        
        Be specific and provide actionable insights.
        ";
        
        return await GenerateResponse(prompt);
    }
    
    public async Task<string> ExploreRepository(string repository)
    {
        try
        {
            // Get repository details
            var repoDetails = await CallMCPTool("get_repository", new { repository });
            
            // Get file structure (top level)
            var fileContents = await CallMCPTool("get_file_contents", new { repository, path = "" });
            
            // Get recent commits
            var commits = await CallMCPTool("list_commits", new { repository, per_page = 10 });
            
            var prompt = $@"
            Analyze this GitHub repository in detail:
            
            Repository Details:
            {repoDetails}
            
            File Structure:
            {fileContents}
            
            Recent Commits:
            {commits}
            
            Provide a comprehensive analysis including:
            1. Project purpose and functionality
            2. Code structure and architecture
            3. Development activity and patterns
            4. Code quality assessment
            5. Areas for improvement
            6. Technology recommendations
            ";
            
            return await GenerateResponse(prompt);
        }
        catch (Exception ex)
        {
            return $"Error exploring repository: {ex.Message}";
        }
    }
}

/// <summary>
/// Issue triage agent for GitHub
/// </summary>
public class IssueTriageAgent : MCPEnabledAgent
{
    public IssueTriageAgent(MCPClient mcpClient, string openAiApiKey, ILogger? logger = null) 
        : base(mcpClient, openAiApiKey, "IssueTriageAgent", 
               """
               You are an issue triage agent responsible for analyzing GitHub issues and providing triage recommendations.
               Your role includes:
               1. Categorizing issues by type (bug, feature request, documentation, etc.)
               2. Assessing priority levels (low, medium, high, critical)
               3. Suggesting appropriate labels
               4. Recommending assignees based on issue content
               5. Identifying duplicate or related issues
               
               Always provide clear, actionable triage recommendations with justifications.
               """, logger)
    {
    }
    
    public async Task<string> TriageIssues(string repository, string issuesData)
    {
        var prompt = $@"
        Analyze and triage the following GitHub issues for repository '{repository}':
        
        Issues Data:
        {issuesData}
        
        For each issue, provide:
        1. Issue category (bug, feature, enhancement, documentation, question, etc.)
        2. Priority level (low, medium, high, critical) with justification
        3. Suggested labels
        4. Estimated complexity (simple, moderate, complex)
        5. Recommended team member or skill set for assignment
        6. Any duplicate or related issues identified
        
        Format your response as a structured triage report.
        ";
        
        return await GenerateResponse(prompt);
    }
    
    public async Task<string> CreateTriageIssue(string repository, string title, string description, string[] labels, string priority)
    {
        try
        {
            var issueBody = $@"
## Issue Description
{description}

## Triage Information
- **Priority**: {priority}
- **Labels**: {string.Join(", ", labels)}
- **Triaged by**: AI Issue Triage Agent
- **Triage Date**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

## Next Steps
This issue has been automatically triaged. Please review and adjust the triage information as needed.
";
            
            var result = await CallMCPTool("create_issue", new { 
                repository, 
                title, 
                body = issueBody,
                labels
            });
            
            return $"Issue created successfully: {result}";
        }
        catch (Exception ex)
        {
            return $"Error creating issue: {ex.Message}";
        }
    }
}

/// <summary>
/// Code review agent for GitHub pull requests
/// </summary>
public class CodeReviewAgent : MCPEnabledAgent
{
    public CodeReviewAgent(MCPClient mcpClient, string openAiApiKey, ILogger? logger = null) 
        : base(mcpClient, openAiApiKey, "CodeReviewAgent", 
               """
               You are an expert code review agent that provides comprehensive code reviews for GitHub pull requests.
               Your responsibilities include:
               1. Analyzing code changes for quality, security, and best practices
               2. Identifying potential bugs and performance issues
               3. Suggesting improvements and optimizations
               4. Ensuring code follows established patterns and conventions
               5. Providing constructive feedback for learning and improvement
               
               Always provide specific, actionable feedback with code examples when possible.
               Be thorough but constructive in your reviews.
               """, logger)
    {
    }
    
    public async Task<string> ReviewPullRequest(string repository, string prNumber, string prData)
    {
        var prompt = $@"
        Review the following GitHub pull request:
        
        Repository: {repository}
        PR Number: {prNumber}
        PR Data:
        {prData}
        
        Please provide a comprehensive code review including:
        1. Overall assessment of the changes
        2. Code quality evaluation
        3. Security considerations
        4. Performance implications
        5. Best practices adherence
        6. Specific suggestions for improvement
        7. Approval recommendation (approve, request changes, or comment)
        
        Format your response as a structured code review with specific line-by-line feedback where applicable.
        ";
        
        return await GenerateResponse(prompt);
    }
    
    public async Task<string> AnalyzeCodeChanges(string repository, string prNumber)
    {
        try
        {
            // Get PR details
            var prDetails = await CallMCPTool("get_pull_request", new { repository, pull_number = int.Parse(prNumber) });
            
            // Get file changes (this would need additional MCP tool support)
            // For now, we'll work with the PR details
            
            var prompt = $@"
            Analyze the code changes in this pull request:
            
            PR Details:
            {prDetails}
            
            Provide:
            1. Summary of changes made
            2. Impact analysis
            3. Risk assessment
            4. Testing recommendations
            5. Deployment considerations
            ";
            
            return await GenerateResponse(prompt);
        }
        catch (Exception ex)
        {
            return $"Error analyzing code changes: {ex.Message}";
        }
    }
}

/// <summary>
/// Development workflow assistant
/// </summary>
public class WorkflowAssistant : MCPEnabledAgent
{
    public WorkflowAssistant(MCPClient mcpClient, string openAiApiKey, ILogger? logger = null) 
        : base(mcpClient, openAiApiKey, "WorkflowAssistant", 
               """
               You are a development workflow assistant that helps teams manage their development processes.
               Your capabilities include:
               1. Planning development tasks and breaking them down into manageable steps
               2. Coordinating between different phases of development
               3. Managing branch strategies and merge workflows
               4. Monitoring project progress and blockers
               5. Facilitating communication between team members
               
               Always provide clear, actionable workflow recommendations and consider team dynamics and project constraints.
               """, logger)
    {
    }
    
    public async Task<string> PlanWorkflow(string repository, string task)
    {
        var prompt = $@"
        Plan a development workflow for the following task:
        
        Repository: {repository}
        Task: {task}
        
        Please provide:
        1. Task breakdown into specific steps
        2. Estimated timeline for each step
        3. Dependencies and prerequisites
        4. Required skills and team members
        5. Testing strategy
        6. Deployment plan
        7. Risk assessment and mitigation
        
        Format as a detailed project plan.
        ";
        
        return await GenerateResponse(prompt);
    }
    
    public async Task<string> ExecuteWorkflow(string repository, string task, string workflowPlan)
    {
        try
        {
            // This is a simulation of workflow execution
            // In a real implementation, this would coordinate actual development tasks
            
            var prompt = $@"
            Execute the following workflow plan:
            
            Repository: {repository}
            Task: {task}
            Workflow Plan:
            {workflowPlan}
            
            Simulate the execution and provide:
            1. Current status of each workflow step
            2. Completed tasks and outcomes
            3. Blockers or issues encountered
            4. Next steps and recommendations
            5. Updated timeline if needed
            
            Format as a workflow execution report.
            ";
            
            return await GenerateResponse(prompt);
        }
        catch (Exception ex)
        {
            return $"Error executing workflow: {ex.Message}";
        }
    }
}
