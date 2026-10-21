using System.Net;
using System.Text;
using Gabriel.Core.Configuration;
using Gabriel.Infrastructure.Tools.Web;
using Gabriel.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gabriel.Tests.Tools;

public class SearxngWebSearchTests
{
    private static SearxngWebSearch Create(HttpMessageHandler handler, string baseUrl = "http://searxng.test/")
        => new(
            new FakeHttpClientFactory(handler, baseUrl),
            Options.Create(new SearxngSearchOptions { BaseUrl = baseUrl }),
            NullLogger<SearxngWebSearch>.Instance);

    private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task Maps_results_and_requests_the_json_format()
    {
        var handler = new RecordingHandler(_ => Json("""
            {"results":[
              {"title":"Feeding ratios","url":"https://a.example/feed","content":"1:1:1 explained","engines":["brave"]},
              {"title":"No url","url":"","content":"dropped"},
              {"title":"Storage","url":"https://b.example/store","content":"fridge tips"}
            ],"unresponsive_engines":[]}
            """));

        var results = await Create(handler).SearchAsync("sourdough feeding", 5, default);

        Assert.Equal(["https://a.example/feed", "https://b.example/store"], results.Select(r => r.Url));
        Assert.Equal("1:1:1 explained", results[0].Snippet);
        var url = handler.Requests.Single().Url;
        Assert.Contains("format=json", url);
        Assert.Contains("q=sourdough%20feeding", url);
        Assert.Contains("categories=general", url);
    }

    [Fact]
    public async Task Respects_the_limit()
    {
        var many = string.Join(",", Enumerable.Range(1, 30).Select(i => $$"""{"title":"t{{i}}","url":"https://x.example/{{i}}","content":""}"""));
        var results = await Create(new RecordingHandler(_ => Json($$"""{"results":[{{many}}]}"""))).SearchAsync("q", 4, default);

        Assert.Equal(4, results.Count);
    }

    [Fact]
    public async Task Explains_a_disabled_json_format()
    {
        var search = Create(new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => search.SearchAsync("q", 5, default));
        Assert.Contains("search.formats", ex.Message);
    }

    [Fact]
    public async Task Explains_an_unreachable_instance()
    {
        var search = Create(new RecordingHandler(_ => throw new HttpRequestException("connection refused")));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => search.SearchAsync("q", 5, default));
        Assert.Contains("unreachable", ex.Message);
        Assert.Contains("docker compose", ex.Message);
    }

    // Opt-in check against a real instance, e.g.
    //   SEARXNG_LIVE_URL=http://127.0.0.1:8888/ dotnet test --filter Live_instance
    // Passes trivially when the variable is unset so CI needs no network.
    [Fact]
    public async Task Live_instance_returns_results_when_configured()
    {
        var url = Environment.GetEnvironmentVariable("SEARXNG_LIVE_URL");
        if (string.IsNullOrWhiteSpace(url)) return;

        using var http = new HttpClientHandler();
        var search = new SearxngWebSearch(
            new FakeHttpClientFactory(http, url),
            Options.Create(new SearxngSearchOptions { BaseUrl = url }),
            NullLogger<SearxngWebSearch>.Instance);

        var results = await search.SearchAsync("dotnet 10 release notes", 5, default);

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.StartsWith("http", r.Url));
    }
}
