public class WebBrowsingConfig
{
    public bool HeadlessMode { get; set; } = true;
    public int DefaultTimeout { get; set; } = 30000;
    public int MaxRetries { get; set; } = 3;
    public string[] AllowedDomains { get; set; } = Array.Empty<string>();
}