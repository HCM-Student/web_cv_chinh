using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace WEB_CV.Services.AI
{
    public class ProtonxSearch : IProtonxSearch
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public ProtonxSearch(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            var baseUrl = cfg["Protonx:BaseUrl"] ?? "http://127.0.0.1:8088";
            _http.BaseAddress = new Uri(baseUrl);
            if (bool.TryParse(cfg["Protonx:UseAuthHeader"], out var useAuth) && useAuth)
            {
                var token = Environment.GetEnvironmentVariable("PROTONX_TOKEN");
                if (!string.IsNullOrWhiteSpace(token))
                    _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
        }

        public async Task<IReadOnlyList<ProtonxHit>> SearchAsync(string query, int n = 5, CancellationToken ct = default)
        {
            try
            {
                var payload = new { query, n_results = n };
                using var resp = await _http.PostAsJsonAsync("/search", payload, ct);
                
                if (!resp.IsSuccessStatusCode)
                {
                    var errorContent = await resp.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"ProtonX API error: {resp.StatusCode} - {errorContent}");
                }
                
                var doc = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                var list = new List<ProtonxHit>();
                
                if (doc.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    foreach (var x in results.EnumerateArray())
                    {
                        list.Add(new ProtonxHit(
                            x.GetProperty("id").GetString() ?? "",
                            x.GetProperty("score").GetDouble(),
                            x.TryGetProperty("title", out var t) ? t.GetString() : null,
                            x.TryGetProperty("url", out var u) ? u.GetString() : null,
                            x.TryGetProperty("snippet", out var s) ? s.GetString() : null
                        ));
                    }
                }
                
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling ProtonX API: {ex.Message}", ex);
            }
        }
    }
}
