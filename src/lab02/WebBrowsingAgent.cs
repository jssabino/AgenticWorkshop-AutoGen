using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Text;
using System.Text.Json;
using DotNetEnv;
using AutoGen;
using AutoGen.Core;
using AutoGen.AzureAIInference;
using AutoGen.AzureAIInference.Extension;
using Azure.AI.Inference;

public class WebBrowsingAgent : IAgent, IAsyncDisposable
{
    private readonly ChatCompletionsClient _openAIClient;
    private readonly WebBrowsingConfig _config;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    public string Name { get; set; } = "WebBrowsingAgent";

    public WebBrowsingAgent(ChatCompletionsClient openAIClient, WebBrowsingConfig config)
    {
        _openAIClient = openAIClient;
        _config = config;
    }

    public async Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return new TextMessage(Role.Assistant, await ProcessMessageAsync(messages.FirstOrDefault()?.GetContent() ?? ""), from: "WebBrowsingAgent");
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _config.HeadlessMode,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        });
        _page = await _browser.NewPageAsync();
        _page.SetDefaultTimeout(_config.DefaultTimeout);
    }

    public async Task<string> ProcessMessageAsync(string message)
    {
        // Extract URL from message if present
        var url = ExtractUrlFromMessage(message);
        if (!string.IsNullOrEmpty(url))
        {
            return await NavigateToUrlAsync(url);
        }

        return "No valid URL found in message for web browsing.";
    }

    private string ExtractUrlFromMessage(string message)
    {
        // Simple URL extraction - in real implementation, use regex or NLP
        var words = message.Split(' ');
        return words.FirstOrDefault(w => w.StartsWith("http")) ?? "";
    }

    public async Task<string> NavigateToUrlAsync(string url)
    {
        if (_page == null)
        {
            await InitializeAsync();
        }

        try
        {
            await _page!.GotoAsync(url);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var title = await _page.TitleAsync();
            var content = await _page.InnerTextAsync("body");

            // Limit content length for processing
            if (content.Length > 2000)
            {
                content = content.Substring(0, 2000) + "...";
            }

            return $"Page Title: {title}\nURL: {url}\nContent: {content}";
        }
        catch (Exception ex)
        {
            return $"Error navigating to {url}: {ex.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        _playwright?.Dispose();
    }
}
