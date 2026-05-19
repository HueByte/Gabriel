using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Gabriel.API.Configuration;

// Pulls secrets from a self-hosted Infisical instance at startup and merges them
// into IConfiguration. Secret keys with `__` separators map to `:` config paths,
// matching the env-var convention - so `PROVIDERS__GROK__APIKEY` in Infisical
// populates `Providers:Grok:ApiKey` for IOptions binding.
public class InfisicalConfigurationProvider : ConfigurationProvider
{
    private readonly InfisicalOptions _opts;

    public InfisicalConfigurationProvider(InfisicalOptions opts)
    {
        _opts = opts;
    }

    public override void Load()
    {
        if (!_opts.IsConfigured) return;

        try
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            // Pre-DI: no logger yet. Use stderr so it shows in console output but
            // doesn't crash startup - missing keys will fail loudly when accessed.
            Console.Error.WriteLine($"[Infisical] secret load failed: {ex.Message}");
        }
    }

    private async Task LoadAsync()
    {
        // IConfigurationProvider.Load runs during builder.Configuration build-out,
        // BEFORE the DI container exists - IHttpClientFactory is not available
        // here. A short-lived HttpClient disposed at the end of this single
        // bootstrap call is the canonical workaround; the usual socket-exhaustion
        // concern doesn't apply because Load is invoked once per process.
        using var http = new HttpClient
        {
            BaseAddress = new Uri(_opts.Host.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(_opts.TimeoutSeconds),
        };

        var token = await AuthenticateAsync(http);
        var secrets = await FetchSecretsAsync(http, token);

        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in secrets)
        {
            data[key.Replace("__", ":")] = value;
        }
        Data = data;
    }

    private async Task<string> AuthenticateAsync(HttpClient http)
    {
        var response = await http.PostAsJsonAsync("api/v1/auth/universal-auth/login", new
        {
            clientId = _opts.ClientId,
            clientSecret = _opts.ClientSecret,
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"auth returned {(int)response.StatusCode}: {Truncate(body)}");
        }

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("auth returned empty body.");
        return payload.AccessToken;
    }

    private async Task<IReadOnlyDictionary<string, string>> FetchSecretsAsync(HttpClient http, string token)
    {
        var url = $"api/v3/secrets/raw" +
                  $"?workspaceId={_opts.ProjectId}" +
                  $"&environment={Uri.EscapeDataString(_opts.Environment)}" +
                  $"&secretPath={Uri.EscapeDataString(_opts.SecretPath)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"secrets fetch returned {(int)response.StatusCode}: {Truncate(body)}");
        }

        var payload = await response.Content.ReadFromJsonAsync<SecretsResponse>();
        if (payload?.Secrets is null) return new Dictionary<string, string>();

        return payload.Secrets.ToDictionary(s => s.SecretKey, s => s.SecretValue);
    }

    private static string Truncate(string s) => s.Length <= 200 ? s : s[..200] + "...";

    private sealed record AuthResponse(
        [property: JsonPropertyName("accessToken")] string AccessToken);

    private sealed record SecretsResponse(
        [property: JsonPropertyName("secrets")] List<SecretEntry>? Secrets);

    private sealed record SecretEntry(
        [property: JsonPropertyName("secretKey")] string SecretKey,
        [property: JsonPropertyName("secretValue")] string SecretValue);
}
