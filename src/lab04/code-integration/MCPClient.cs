using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lab04CodeIntegration;

/// <summary>
/// MCP (Model Context Protocol) Client for communicating with MCP servers
/// </summary>
public class MCPClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MCPClient> _logger;
    private readonly string _mcpServerUrl;
    
    public MCPClient(HttpClient httpClient, ILogger<MCPClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _mcpServerUrl = Environment.GetEnvironmentVariable("MCP_SERVER_URL") ?? "http://localhost:3000";
    }
    
    /// <summary>
    /// List all available tools from the MCP server
    /// </summary>
    public async Task<List<MCPTool>> ListTools()
    {
        try
        {
            var request = new MCPRequest
            {
                Method = "tools/list",
                Params = new { }
            };
            
            var response = await SendRequest(request);
            
            if (response.Success && response.Result != null)
            {
                var tools = JsonConvert.DeserializeObject<List<MCPTool>>(response.Result.ToString());
                return tools ?? new List<MCPTool>();
            }
            
            return new List<MCPTool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list MCP tools");
            return new List<MCPTool>();
        }
    }
    
    /// <summary>
    /// Call a specific tool with parameters
    /// </summary>
    public async Task<object> CallTool(string toolName, object parameters)
    {
        try
        {
            var request = new MCPRequest
            {
                Method = "tools/call",
                Params = new
                {
                    name = toolName,
                    arguments = parameters
                }
            };
            
            var response = await SendRequest(request);
            
            if (response.Success)
            {
                return response.Result ?? "Tool executed successfully";
            }
            
            throw new MCPException($"Tool call failed: {response.Error}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call MCP tool: {ToolName}", toolName);
            throw;
        }
    }
    
    /// <summary>
    /// Initialize connection with the MCP server
    /// </summary>
    public async Task<bool> Initialize()
    {
        try
        {
            var request = new MCPRequest
            {
                Method = "initialize",
                Params = new
                {
                    protocolVersion = "2024-11-05",
                    capabilities = new
                    {
                        tools = new { },
                        resources = new { }
                    },
                    clientInfo = new
                    {
                        name = "AutoGen-MCP-Client",
                        version = "1.0.0"
                    }
                }
            };
            
            var response = await SendRequest(request);
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MCP connection");
            return false;
        }
    }
    
    /// <summary>
    /// Send a request to the MCP server
    /// </summary>
    private async Task<MCPResponse> SendRequest(MCPRequest request)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_mcpServerUrl}/mcp", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var mcpResponse = JsonConvert.DeserializeObject<MCPResponse>(responseContent);
                return mcpResponse ?? new MCPResponse { Success = false, Error = "Invalid response format" };
            }
            
            return new MCPResponse
            {
                Success = false,
                Error = $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send MCP request");
            return new MCPResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}

/// <summary>
/// MCP request structure
/// </summary>
public class MCPRequest
{
    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;
    
    [JsonProperty("params")]
    public object? Params { get; set; }
}

/// <summary>
/// MCP response structure
/// </summary>
public class MCPResponse
{
    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("result")]
    public object? Result { get; set; }
    
    [JsonProperty("error")]
    public string? Error { get; set; }
    
    [JsonIgnore]
    public bool Success => Error == null;
}

/// <summary>
/// MCP tool definition
/// </summary>
public class MCPTool
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty("inputSchema")]
    public object? InputSchema { get; set; }
}

/// <summary>
/// MCP-specific exception
/// </summary>
public class MCPException : Exception
{
    public MCPException(string message) : base(message) { }
    public MCPException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// MCP operation result
/// </summary>
public class MCPResult
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// GitHub-specific MCP operations
/// </summary>
public static class GitHubMCPOperations
{
    public static readonly string[] AvailableOperations = new[]
    {
        "list_repositories",
        "get_repository",
        "list_issues",
        "create_issue",
        "get_issue",
        "update_issue",
        "list_pull_requests",
        "get_pull_request",
        "create_pull_request",
        "get_file_contents",
        "create_file",
        "update_file",
        "delete_file",
        "list_branches",
        "create_branch",
        "get_commit",
        "list_commits"
    };
    
    public static bool IsValidOperation(string operation)
    {
        return AvailableOperations.Contains(operation);
    }
}
