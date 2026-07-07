# TavilyWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/TavilyWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class TavilyWebSearch : IWebSearch
```


This class implements the IWebSearch interface to provide web search capabilities backed by the Tavily Search API (tavily.com). It is designed for scenarios where an application needs to perform web searches optimized for language model consumption, returning a pre-ranked list of concise search results including title, URL, and snippet content.

TavilyWebSearch sends POST requests with a JSON payload containing the query, API key, and search parameters, and authenticates using a Bearer token. The API response is parsed into a list of search results that are already trimmed and scored for efficient downstream processing by language models. This avoids the need for additional truncation or re-ranking on the client side.

## Remarks

This implementation emphasizes flexibility and robustness by attaching the Bearer token per request rather than as a default header, allowing seamless API key rotation without restarting the application. It also opts out of including raw content, images, or direct answers in the response to keep the payload lightweight and maintain citation integrity for reasoning steps. TavilyWebSearch fits into a toolset where concise, snippet-sized search results are preferred to support agent workflows or AI-driven search scenarios.