using Microsoft.AutoGen.Abstractions;
using Microsoft.AutoGen.Agents;
using Microsoft.AutoGen.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNetEnv;

namespace Lab02;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load();
        
        Console.WriteLine("🚀 AutoGen Lab 02: Multi-Agent Systems");
        Console.WriteLine("======================================");
        
        try
        {
            // Configure services
            var host = CreateHostBuilder(args).Build();
            
            // Get the MultiAgent service
            var multiAgentService = host.Services.GetRequiredService<MultiAgentService>();
            
            // Run different multi-agent scenarios
            await RunScenarioMenu(multiAgentService);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("\n💡 Make sure you have set your OpenAI API key in the .env file");
        }
        
        Console.WriteLine("\n👋 Thanks for trying AutoGen Lab 02!");
    }
    
    static async Task RunScenarioMenu(MultiAgentService service)
    {
        while (true)
        {
            Console.WriteLine("\n🎮 Choose a Multi-Agent Scenario:");
            Console.WriteLine("1. Software Development Team");
            Console.WriteLine("2. Research Team");
            Console.WriteLine("3. Customer Support Team");
            Console.WriteLine("4. Debate System");
            Console.WriteLine("5. Exit");
            
            Console.Write("\nEnter your choice (1-5): ");
            var choice = Console.ReadLine();
            
            try
            {
                switch (choice)
                {
                    case "1":
                        await service.RunSoftwareDevelopmentTeam();
                        break;
                    case "2":
                        await service.RunResearchTeam();
                        break;
                    case "3":
                        await service.RunCustomerSupportTeam();
                        break;
                    case "4":
                        await service.RunDebateSystem();
                        break;
                    case "5":
                        Console.WriteLine("👋 Goodbye!");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error running scenario: {ex.Message}");
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
                services.AddSingleton<MultiAgentService>();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });
}

public class MultiAgentService
{
    private readonly ILogger<MultiAgentService> _logger;
    private readonly string _apiKey;
    
    public MultiAgentService(ILogger<MultiAgentService> logger)
    {
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("OpenAI API key not found.");
    }
    
    public async Task RunSoftwareDevelopmentTeam()
    {
        Console.WriteLine("\n💻 Software Development Team Scenario");
        Console.WriteLine("=====================================");
        
        // Create specialized agents
        var productOwner = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "ProductOwner",
            systemMessage: """
                You are a Product Owner responsible for defining software requirements.
                You create clear, actionable user stories and acceptance criteria.
                You prioritize features based on business value and user needs.
                Be concise and focus on the 'what' and 'why' rather than the 'how'.
                """
        );
        
        var developer = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "Developer",
            systemMessage: """
                You are a Senior Software Developer specialized in C#.
                You write clean, maintainable code following best practices.
                You ask clarifying questions about requirements when needed.
                You suggest technical solutions and explain your reasoning.
                """
        );
        
        var codeReviewer = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "CodeReviewer",
            systemMessage: """
                You are a Senior Code Reviewer focused on code quality.
                You review code for best practices, security, performance, and maintainability.
                You provide constructive feedback and suggest improvements.
                You approve code only when it meets high standards.
                """
        );
        
        var tester = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "Tester",
            systemMessage: """
                You are a QA Engineer responsible for testing software.
                You create comprehensive test cases and scenarios.
                You identify edge cases and potential issues.
                You verify that implementations match requirements.
                """
        );
        
        // Simulate a software development workflow
        var task = "Create a simple calculator application with basic arithmetic operations";
        
        Console.WriteLine($"📋 Task: {task}");
        Console.WriteLine("\n🔄 Starting development workflow...");
        
        // Step 1: Product Owner defines requirements
        Console.WriteLine("\n👤 Product Owner - Defining Requirements:");
        var requirementsResponse = await productOwner.GenerateReplyAsync(
            new List<IMessage> { new TextMessage(Role.User, 
                $"Please define detailed requirements for: {task}") }
        );
        Console.WriteLine($"📝 {requirementsResponse.GetContent()}");
        
        // Step 2: Developer implements solution
        Console.WriteLine("\n👨‍💻 Developer - Implementing Solution:");
        var developmentResponse = await developer.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, requirementsResponse.GetContent()),
                new TextMessage(Role.User, "Please implement this solution in C#")
            }
        );
        Console.WriteLine($"🔧 {developmentResponse.GetContent()}");
        
        // Step 3: Code Reviewer reviews the code
        Console.WriteLine("\n🔍 Code Reviewer - Reviewing Code:");
        var reviewResponse = await codeReviewer.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, developmentResponse.GetContent()),
                new TextMessage(Role.User, "Please review this code for quality and best practices")
            }
        );
        Console.WriteLine($"📊 {reviewResponse.GetContent()}");
        
        // Step 4: Tester creates test cases
        Console.WriteLine("\n🧪 Tester - Creating Test Cases:");
        var testResponse = await tester.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, requirementsResponse.GetContent()),
                new TextMessage(Role.User, developmentResponse.GetContent()),
                new TextMessage(Role.User, "Please create comprehensive test cases for this implementation")
            }
        );
        Console.WriteLine($"✅ {testResponse.GetContent()}");
        
        Console.WriteLine("\n🎉 Software Development Team workflow completed!");
    }
    
    public async Task RunResearchTeam()
    {
        Console.WriteLine("\n🔬 Research Team Scenario");
        Console.WriteLine("=========================");
        
        var researchLead = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "ResearchLead",
            systemMessage: """
                You are a Research Lead coordinating research projects.
                You define research objectives and methodologies.
                You guide the team toward meaningful insights.
                You synthesize findings from different team members.
                """
        );
        
        var dataAnalyst = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "DataAnalyst",
            systemMessage: """
                You are a Data Analyst specializing in statistical analysis.
                You analyze data patterns and trends.
                You create visualizations and interpret results.
                You ensure data quality and statistical validity.
                """
        );
        
        var writer = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "Writer",
            systemMessage: """
                You are a Research Writer specializing in technical communication.
                You synthesize complex information into clear, readable reports.
                You ensure logical flow and proper citations.
                You adapt writing style to the target audience.
                """
        );
        
        var topic = "The impact of AI on software development productivity";
        
        Console.WriteLine($"📊 Research Topic: {topic}");
        Console.WriteLine("\n🔄 Starting research workflow...");
        
        // Research workflow simulation
        Console.WriteLine("\n👨‍🔬 Research Lead - Defining Research Plan:");
        var planResponse = await researchLead.GenerateReplyAsync(
            new List<IMessage> { new TextMessage(Role.User, 
                $"Create a research plan for: {topic}") }
        );
        Console.WriteLine($"📋 {planResponse.GetContent()}");
        
        Console.WriteLine("\n📊 Data Analyst - Analyzing Data:");
        var analysisResponse = await dataAnalyst.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, planResponse.GetContent()),
                new TextMessage(Role.User, "Provide a data analysis approach for this research")
            }
        );
        Console.WriteLine($"📈 {analysisResponse.GetContent()}");
        
        Console.WriteLine("\n✍️ Writer - Creating Report:");
        var reportResponse = await writer.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, planResponse.GetContent()),
                new TextMessage(Role.User, analysisResponse.GetContent()),
                new TextMessage(Role.User, "Write an executive summary based on this research")
            }
        );
        Console.WriteLine($"📄 {reportResponse.GetContent()}");
        
        Console.WriteLine("\n🎉 Research Team workflow completed!");
    }
    
    public async Task RunCustomerSupportTeam()
    {
        Console.WriteLine("\n🎧 Customer Support Team Scenario");
        Console.WriteLine("==================================");
        
        var intakeAgent = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "IntakeAgent",
            systemMessage: """
                You are a Customer Support Intake Agent.
                You receive and categorize customer inquiries.
                You gather initial information and determine the appropriate specialist.
                You are friendly, patient, and thorough in understanding customer needs.
                """
        );
        
        var technicalExpert = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "TechnicalExpert",
            systemMessage: """
                You are a Technical Support Expert.
                You solve complex technical issues and provide detailed solutions.
                You explain technical concepts in user-friendly terms.
                You escalate issues when they're beyond your expertise.
                """
        );
        
        var customerIssue = "My application crashes when I try to save large files";
        
        Console.WriteLine($"📞 Customer Issue: {customerIssue}");
        Console.WriteLine("\n🔄 Starting support workflow...");
        
        Console.WriteLine("\n📋 Intake Agent - Processing Request:");
        var intakeResponse = await intakeAgent.GenerateReplyAsync(
            new List<IMessage> { new TextMessage(Role.User, 
                $"Customer reports: {customerIssue}. Please categorize and gather initial information.") }
        );
        Console.WriteLine($"📝 {intakeResponse.GetContent()}");
        
        Console.WriteLine("\n🔧 Technical Expert - Providing Solution:");
        var solutionResponse = await technicalExpert.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, intakeResponse.GetContent()),
                new TextMessage(Role.User, "Please provide a technical solution for this issue")
            }
        );
        Console.WriteLine($"💡 {solutionResponse.GetContent()}");
        
        Console.WriteLine("\n🎉 Customer Support workflow completed!");
    }
    
    public async Task RunDebateSystem()
    {
        Console.WriteLine("\n🗣️ Debate System Scenario");
        Console.WriteLine("=========================");
        
        var debaterA = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "DebaterA",
            systemMessage: """
                You are a skilled debater arguing FOR the proposition.
                You present logical arguments with evidence and examples.
                You counter opposing arguments respectfully and effectively.
                You maintain a professional and persuasive tone.
                """
        );
        
        var debaterB = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "DebaterB",
            systemMessage: """
                You are a skilled debater arguing AGAINST the proposition.
                You present logical counter-arguments with evidence and examples.
                You challenge the opposing side's arguments respectfully.
                You maintain a professional and persuasive tone.
                """
        );
        
        var moderator = new OpenAIChatAgent(
            chatClient: new OpenAI.Chat.ChatClient("gpt-4", _apiKey),
            name: "Moderator",
            systemMessage: """
                You are a neutral debate moderator.
                You ensure fair discussion and evaluate arguments objectively.
                You summarize key points and maintain debate structure.
                You provide balanced analysis without taking sides.
                """
        );
        
        var debateTopic = "Remote work is more productive than office work";
        
        Console.WriteLine($"🎯 Debate Topic: {debateTopic}");
        Console.WriteLine("\n🔄 Starting debate...");
        
        Console.WriteLine("\n👨‍💼 Moderator - Opening the Debate:");
        var openingResponse = await moderator.GenerateReplyAsync(
            new List<IMessage> { new TextMessage(Role.User, 
                $"Please open a debate on: {debateTopic}") }
        );
        Console.WriteLine($"🎤 {openingResponse.GetContent()}");
        
        Console.WriteLine("\n✅ Debater A - Arguing FOR:");
        var argumentA = await debaterA.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, openingResponse.GetContent()),
                new TextMessage(Role.User, "Present your opening argument FOR the proposition")
            }
        );
        Console.WriteLine($"💬 {argumentA.GetContent()}");
        
        Console.WriteLine("\n❌ Debater B - Arguing AGAINST:");
        var argumentB = await debaterB.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, openingResponse.GetContent()),
                new TextMessage(Role.User, argumentA.GetContent()),
                new TextMessage(Role.User, "Present your opening argument AGAINST the proposition")
            }
        );
        Console.WriteLine($"💬 {argumentB.GetContent()}");
        
        Console.WriteLine("\n👨‍💼 Moderator - Summarizing Debate:");
        var summaryResponse = await moderator.GenerateReplyAsync(
            new List<IMessage> { 
                new TextMessage(Role.User, openingResponse.GetContent()),
                new TextMessage(Role.User, argumentA.GetContent()),
                new TextMessage(Role.User, argumentB.GetContent()),
                new TextMessage(Role.User, "Please provide a neutral summary of both arguments")
            }
        );
        Console.WriteLine($"📊 {summaryResponse.GetContent()}");
        
        Console.WriteLine("\n🎉 Debate completed!");
    }
}

// Helper classes
public class TextMessage : IMessage
{
    public Role Role { get; }
    public string Content { get; }
    
    public TextMessage(Role role, string content)
    {
        Role = role;
        Content = content;
    }
    
    public string GetContent() => Content;
}

public enum Role
{
    User,
    Assistant,
    System
}
