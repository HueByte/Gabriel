using System.Net;
using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Infrastructure.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gabriel.Tests.Memory;

public class QdrantMemoryIndexTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProjectId = Guid.NewGuid();

    private static MemoryEntry Entry(Guid? projectId = null) => MemoryEntry.Create(
        userId: UserId,
        projectId: projectId,
        type: MemoryEntryType.Feedback,
        name: "prefers-prose",
        description: "write prose not bullets",
        body: "The user prefers prose in docs and replies.");

    private static QdrantMemoryIndex Create(RecordingHandler handler)
    {
        var options = Options.Create(new SemanticMemoryOptions { Enabled = true });
        var embeddings = new MockEmbeddingProvider(Options.Create(new EmbeddingOptions()));
        return new QdrantMemoryIndex(
            new FakeHttpClientFactory(handler),
            embeddings,
            options,
            NullLogger<QdrantMemoryIndex>.Instance);
    }

    [Fact]
    public async Task Upsert_creates_collection_then_puts_point_with_scope_payload()
    {
        var handler = new RecordingHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            return Ok("""{"result":true,"status":"ok"}""");
        });

        await Create(handler).UpsertAsync(Entry(projectId: ProjectId));

        // GET probe, PUT collection, 2x PUT index, PUT points.
        Assert.Equal(5, handler.Requests.Count);
        var pointsRequest = handler.Requests[^1];
        Assert.Contains("points?wait=true", pointsRequest.Url);

        using var doc = JsonDocument.Parse(pointsRequest.Body!);
        var point = doc.RootElement.GetProperty("points")[0];
        var payload = point.GetProperty("payload");
        Assert.Equal(UserId.ToString(), payload.GetProperty("userId").GetString());
        Assert.Equal(ProjectId.ToString(), payload.GetProperty("projectId").GetString());
    }

    [Fact]
    public async Task Upsert_user_scope_uses_sentinel_project_payload()
    {
        var handler = new RecordingHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            return Ok("""{"result":true,"status":"ok"}""");
        });

        await Create(handler).UpsertAsync(Entry(projectId: null));

        using var doc = JsonDocument.Parse(handler.Requests[^1].Body!);
        var payload = doc.RootElement.GetProperty("points")[0].GetProperty("payload");
        Assert.Equal("__user__", payload.GetProperty("projectId").GetString());
    }

    [Fact]
    public async Task Search_filters_by_user_and_project_and_parses_hits()
    {
        var hitId = Guid.NewGuid();
        var handler = new RecordingHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
                return Ok("""{"result":{"status":"green"}}""");
            if (request.RequestUri!.AbsolutePath.EndsWith("/points/search"))
                return Ok($$"""{"result":[{"id":"{{hitId}}","score":0.87}]}""");
            return Ok("""{"result":true}""");
        });

        var hits = await Create(handler).SearchAsync("prose style", UserId, ProjectId, topK: 5, minScore: 0.3f);

        var hit = Assert.Single(hits);
        Assert.Equal(hitId, hit.MemoryId);
        Assert.Equal(0.87f, hit.Score, precision: 2);

        var searchRequest = handler.Requests.Single(r => r.Url.EndsWith("/points/search"));
        using var doc = JsonDocument.Parse(searchRequest.Body!);
        Assert.Equal(5, doc.RootElement.GetProperty("limit").GetInt32());
        var must = doc.RootElement.GetProperty("filter").GetProperty("must");
        Assert.Equal(UserId.ToString(), must[0].GetProperty("match").GetProperty("value").GetString());
        var any = must[1].GetProperty("match").GetProperty("any").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("__user__", any);
        Assert.Contains(ProjectId.ToString(), any);
    }

    [Fact]
    public async Task Index_degrades_instead_of_throwing_when_qdrant_is_down()
    {
        var handler = new RecordingHandler(_ => throw new HttpRequestException("connection refused"));
        var index = Create(handler);

        await index.UpsertAsync(Entry());
        await index.DeleteAsync(Guid.NewGuid());
        var hits = await index.SearchAsync("anything", UserId, null, 5, 0.3f);
        var written = await index.ReindexAsync([Entry()]);

        Assert.Empty(hits);
        Assert.Equal(0, written);
    }

    private static HttpResponseMessage Ok(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    private sealed record RecordedRequest(HttpMethod Method, string Url, string? Body);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public List<RecordedRequest> Requests { get; } = [];

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            => _responder = responder;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RecordedRequest(request.Method, request.RequestUri!.ToString(), body));
            return _responder(request);
        }
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;
        public FakeHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false)
        {
            BaseAddress = new Uri("http://localhost:6333/"),
        };
    }
}
