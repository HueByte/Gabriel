namespace Gabriel.Core.Configuration;

// Self-hosted SearXNG metasearch (https://docs.searxng.org). No API key: the
// instance queries many engines itself and spreads the load, so it holds up
// where scraping a single engine (DuckDuckGo) gets an IP flagged. The docker
// compose stack runs one; outside Docker, start just that service.
public class SearxngSearchOptions : IConfigSection<SearxngSearchOptions>
{
    public static string SectionName => "Tools:Web:Searxng";

    // Instance root, trailing slash. The compose network name is used in
    // Docker (http://searxng:8080/); the default suits a dev box running the
    // compose service with its published port.
    public string BaseUrl { get; set; } = "http://localhost:8888/";

    public int TimeoutSeconds { get; set; } = 15;

    // SearXNG category to search. "general" is the plain web results tab.
    public string Categories { get; set; } = "general";
}
