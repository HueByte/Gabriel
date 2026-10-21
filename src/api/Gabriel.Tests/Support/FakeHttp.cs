namespace Gabriel.Tests.Support;

internal sealed record RecordedRequest(HttpMethod Method, string Url, string? Body);

// Captures every request (body read eagerly, before the content is disposed)
// and answers from a caller-supplied responder.
internal sealed class RecordingHandler : HttpMessageHandler
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
        // AbsoluteUri, not ToString(): the escaped form that actually goes on the wire.
        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!.AbsoluteUri, body));
        return _responder(request);
    }
}

// Every named client shares one handler; BaseAddress stands in for the
// per-name configuration DependencyInjection would normally apply.
internal sealed class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;
    private readonly Uri _baseAddress;

    public FakeHttpClientFactory(HttpMessageHandler handler, string baseAddress = "http://localhost/")
    {
        _handler = handler;
        _baseAddress = new Uri(baseAddress);
    }

    public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false)
    {
        BaseAddress = _baseAddress,
    };
}
