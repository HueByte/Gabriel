using System.Net;
using System.Text;
using Gabriel.Core.Configuration;
using Gabriel.Infrastructure.Tools.Docs;
using Gabriel.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gabriel.Tests.Tools;

public class GitHubDocsLookupTests
{
    // docs/README.md (human docs map) and docs/gabriel-self-docs/README.md
    // (self-docs index) collide on the bare name - the case the namespace exists for.
    private const string TreeJson = """
        {"tree":[
          {"path":"docs/README.md","type":"blob"},
          {"path":"docs/Gabriel.Engine/tools.md","type":"blob"},
          {"path":"docs/gabriel-self-docs/README.md","type":"blob"},
          {"path":"docs/gabriel-self-docs/tools.md","type":"blob"},
          {"path":"docs/gabriel-self-docs/notes.txt","type":"blob"},
          {"path":"docs/Gabriel.Engine","type":"tree"},
          {"path":"src/README.md","type":"blob"}
        ]}
        """;

    private static (GitHubDocsLookup Lookup, RecordingHandler Handler) Create(bool treeFails = false)
    {
        var handler = new RecordingHandler(request =>
        {
            var url = request.RequestUri!.ToString();
            if (url.Contains("git/trees"))
            {
                return treeFails
                    ? new HttpResponseMessage(HttpStatusCode.Forbidden)
                    : new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(TreeJson, Encoding.UTF8, "application/json"),
                    };
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"body of {url}") };
        });
        var lookup = new GitHubDocsLookup(
            new FakeHttpClientFactory(handler, "https://api.github.com/"),
            Options.Create(new GitHubDocsOptions()),
            NullLogger<GitHubDocsLookup>.Instance);
        return (lookup, handler);
    }

    [Fact]
    public async Task List_uses_bare_names_for_self_docs_and_docs_relative_paths_otherwise()
    {
        var (lookup, _) = Create();

        var paths = (await lookup.ListAsync(CancellationToken.None)).Select(e => e.Path).ToList();

        // Self-docs first; docs/README.md is shadowed by the self-docs README.
        Assert.Equal(["README.md", "tools.md", "Gabriel.Engine/tools.md"], paths);
    }

    [Theory]
    [InlineData("README.md", "docs/gabriel-self-docs/README.md")]
    [InlineData("tools.md", "docs/gabriel-self-docs/tools.md")]
    [InlineData("Gabriel.Engine/tools.md", "docs/Gabriel.Engine/tools.md")]
    public async Task Read_resolves_namespace_path_to_the_listed_repo_file(string path, string repoPath)
    {
        var (lookup, handler) = Create();

        var content = await lookup.ReadAsync(path, CancellationToken.None);

        Assert.NotNull(content);
        Assert.Equal(path, content.Path);
        Assert.EndsWith($"/main/{repoPath}", handler.Requests[^1].Url);
    }

    [Fact]
    public async Task Read_of_unlisted_path_returns_null_without_fetching()
    {
        var (lookup, handler) = Create();

        Assert.Null(await lookup.ReadAsync("missing.md", CancellationToken.None));
        Assert.DoesNotContain(handler.Requests, r => r.Url.Contains("raw.githubusercontent.com"));
    }

    [Fact]
    public async Task Read_falls_back_to_self_docs_when_listing_is_unavailable()
    {
        var (lookup, handler) = Create(treeFails: true);

        var content = await lookup.ReadAsync("agent-loop.md", CancellationToken.None);

        Assert.NotNull(content);
        Assert.EndsWith("/main/docs/gabriel-self-docs/agent-loop.md", handler.Requests[^1].Url);
    }

    [Theory]
    [InlineData("../secrets.md")]
    [InlineData("/etc/passwd")]
    public async Task Read_rejects_traversal(string path)
    {
        var (lookup, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(() => lookup.ReadAsync(path, CancellationToken.None));
    }
}
