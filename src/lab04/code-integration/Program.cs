using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNetEnv;

namespace Lab04CodeIntegration;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load();
        
        Console.WriteLine("🚀 AutoGen Lab 04: Code Integration with GitHub MCP");
        Console.WriteLine("====================================================");
        
        try
        {
            // Configure services
            var host = CreateHostBuilder(args).Build();
            
            // Get the MCP integration service
            var mcpService = host.Services.GetRequiredService<MCPIntegrationService>();
            
            // Run the MCP integration demo
            await RunMCPDemoMenu(mcpService);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("\n💡 Make sure you have:");
            Console.WriteLine("   - Set your GitHub token in the .env file");
            Console.WriteLine("   - Set your OpenAI API key in the .env file");
            Console.WriteLine("   - Installed the GitHub MCP server (npm install @modelcontextprotocol/server-github)");
        }
        
        Console.WriteLine("\n👋 Thanks for trying AutoGen Lab 04 Code Integration!");
    }
    
    static async Task RunMCPDemoMenu(MCPIntegrationService service)
    {
        while (true)
        {
            Console.WriteLine("\n🎮 Choose an MCP Integration Demo:");
            Console.WriteLine("1. Repository Explorer");
            Console.WriteLine("2. Automated Issue Triage");
            Console.WriteLine("3. AI Code Reviewer");
            Console.WriteLine("4. Development Workflow Assistant");
            Console.WriteLine("5. Test MCP Connection");
            Console.WriteLine("6. Exit");
            
            Console.Write("\nEnter your choice (1-6): ");
            var choice = Console.ReadLine();
            
            try
            {
                switch (choice)
                {
                    case "1":
                        await service.RunRepositoryExplorer();
                        break;
                    case "2":
                        await service.RunIssueTriageDemo();
                        break;
                    case "3":
                        await service.RunCodeReviewDemo();
                        break;
                    case "4":
                        await service.RunWorkflowAssistant();
                        break;
                    case "5":
                        await service.TestMCPConnection();
                        break;
                    case "6":
                        Console.WriteLine("👋 Goodbye!");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error running demo: {ex.Message}");
                Console.WriteLine("💡 Check your configuration and try again.");
            }
        }
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<MCPClient>();
                services.AddSingleton<MCPIntegrationService>();
                services.AddHttpClient();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}

public class MCPIntegrationService
{
    private readonly ILogger<MCPIntegrationService> _logger;
    private readonly MCPClient _mcpClient;
    private readonly string _githubToken;
    private readonly string _openAiApiKey;
    
    public MCPIntegrationService(ILogger<MCPIntegrationService> logger, MCPClient mcpClient)
    {
        _logger = logger;
        _mcpClient = mcpClient;
        _githubToken = Environment.GetEnvironmentVariable("GITHUB_PERSONAL_ACCESS_TOKEN") 
            ?? throw new InvalidOperationException("GitHub token not found.");
        _openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("OpenAI API key not found.");
    }
    
    public async Task TestMCPConnection()
    {
        Console.WriteLine("\n🔧 Testing MCP Connection...");
        
        try
        {
            // Test MCP server connection
            var tools = await _mcpClient.ListTools();
            Console.WriteLine($"✅ MCP Server connected successfully!");
            Console.WriteLine($"📊 Available tools: {tools.Count}");
            
            foreach (var tool in tools)
            {
                Console.WriteLine($"   🔧 {tool.Name}: {tool.Description}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MCP Connection failed: {ex.Message}");
            Console.WriteLine("💡 Make sure the GitHub MCP server is running:");
            Console.WriteLine("   npx @modelcontextprotocol/server-github");
        }
    }
    
    public async Task RunRepositoryExplorer()
    {
        Console.WriteLine("\n🔍 Repository Explorer Demo");
        Console.WriteLine("============================");
        
        Console.Write("Enter GitHub username or organization: ");
        var username = Console.ReadLine();
        
        if (string.IsNullOrEmpty(username))
        {
            Console.WriteLine("❌ Username is required.");
            return;
        }
        
        try
        {
            // Create GitHub explorer agent
            var explorerAgent = new GitHubExplorerAgent(_mcpClient, _openAiApiKey);
            
            Console.WriteLine($"\n🤖 Explorer Agent analyzing repositories for {username}...");
            
            // Get repositories
            var repos = await _mcpClient.CallTool("list_repositories", new { username = username });
            Console.WriteLine($"📚 Found repositories: {repos}");
            
            // Let the agent analyze the repositories
            var analysis = await explorerAgent.AnalyzeRepositories(username, repos.ToString());
            Console.WriteLine($"\n📊 Agent Analysis:");
            Console.WriteLine($"{analysis}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Repository exploration failed: {ex.Message}");
        }
    }
    
    public async Task RunIssueTriageDemo()
    {
        Console.WriteLine("\n🎯 Automated Issue Triage Demo");
        Console.WriteLine("===============================");
        
        Console.Write("Enter repository (format: owner/repo): ");
        var repository = Console.ReadLine();
        
        if (string.IsNullOrEmpty(repository))
        {
            Console.WriteLine("❌ Repository is required.");
            return;
        }
        
        try
        {
            // Create issue triage agent
            var triageAgent = new IssueTriageAgent(_mcpClient, _openAiApiKey);
            
            Console.WriteLine($"\n🤖 Triage Agent analyzing issues for {repository}...");
            
            // Get recent issues
            var issues = await _mcpClient.CallTool("list_issues", new { repository = repository });
            Console.WriteLine($"📝 Found issues: {issues}");
            
            // Let the agent triage the issues
            var triageResult = await triageAgent.TriageIssues(repository, issues.ToString());
            Console.WriteLine($"\n🏷️ Triage Results:");
            Console.WriteLine($"{triageResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Issue triage failed: {ex.Message}");
        }
    }
    
    public async Task RunCodeReviewDemo()
    {
        Console.WriteLine("\n👨‍💻 AI Code Reviewer Demo");
        Console.WriteLine("===========================");
        
        Console.Write("Enter repository (format: owner/repo): ");
        var repository = Console.ReadLine();
        
        Console.Write("Enter pull request number: ");
        var prNumber = Console.ReadLine();
        
        if (string.IsNullOrEmpty(repository) || string.IsNullOrEmpty(prNumber))
        {
            Console.WriteLine("❌ Repository and PR number are required.");
            return;
        }
        
        try
        {
            // Create code review agent
            var reviewAgent = new CodeReviewAgent(_mcpClient, _openAiApiKey);
            
            Console.WriteLine($"\n🤖 Code Review Agent analyzing PR #{prNumber} in {repository}...");
            
            // Get PR details
            var prDetails = await _mcpClient.CallTool("get_pull_request", new { 
                repository = repository, 
                pull_number = int.Parse(prNumber) 
            });
            
            Console.WriteLine($"📋 PR Details retrieved: {prDetails}");
            
            // Let the agent review the code
            var reviewResult = await reviewAgent.ReviewPullRequest(repository, prNumber, prDetails.ToString());
            Console.WriteLine($"\n📝 Code Review:");
            Console.WriteLine($"{reviewResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Code review failed: {ex.Message}");
        }
    }
    
    public async Task RunWorkflowAssistant()
    {
        Console.WriteLine("\n🔄 Development Workflow Assistant");
        Console.WriteLine("==================================");
        
        Console.Write("Enter repository (format: owner/repo): ");
        var repository = Console.ReadLine();
        
        Console.Write("Enter task description: ");
        var task = Console.ReadLine();
        
        if (string.IsNullOrEmpty(repository) || string.IsNullOrEmpty(task))
        {
            Console.WriteLine("❌ Repository and task description are required.");
            return;
        }
        
        try
        {
            // Create workflow assistant
            var workflowAssistant = new WorkflowAssistant(_mcpClient, _openAiApiKey);
            
            Console.WriteLine($"\n🤖 Workflow Assistant planning task: {task}");
            
            // Plan the workflow
            var workflowPlan = await workflowAssistant.PlanWorkflow(repository, task);
            Console.WriteLine($"\n📋 Workflow Plan:");
            Console.WriteLine($"{workflowPlan}");
            
            // Execute the workflow (simulation)
            Console.WriteLine($"\n🚀 Executing workflow...");
            var executionResult = await workflowAssistant.ExecuteWorkflow(repository, task, workflowPlan);
            Console.WriteLine($"\n✅ Execution Results:");
            Console.WriteLine($"{executionResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Workflow execution failed: {ex.Message}");
        }
    }
}
