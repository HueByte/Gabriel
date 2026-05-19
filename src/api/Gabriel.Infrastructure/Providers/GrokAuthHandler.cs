using System.Net.Http.Headers;
using Gabriel.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Providers;

// Attaches the Grok API key as a Bearer token on every outbound request sent
// through the named Grok HttpClient. Reads via IOptionsMonitor so a rotated
// key (e.g. swapped via Infisical and a reload signal) takes effect without
// recycling the pooled handler.
internal sealed class GrokAuthHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<GrokOptions> _options;

    public GrokAuthHandler(IOptionsMonitor<GrokOptions> options)
    {
        _options = options;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var apiKey = _options.CurrentValue.ApiKey;
        if (!string.IsNullOrEmpty(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
