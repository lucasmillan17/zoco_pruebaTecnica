using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CMS.Application.Exceptions;
using CMS.Application.Oportunidad;
using Microsoft.Extensions.Configuration;

namespace CMS.Infrastructure.Ai;

/// <summary>
/// Cliente del proveedor Gemini (Google). Usa la API generateContent con
/// responseMimeType application/json para obtener JSON estructurado.
/// </summary>
public class GeminiClient : IGeminiClient
{
    private const string Modelo = "gemini-3.5-flash";
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";

    private readonly HttpClient _http;
    private readonly string? _apiKey;

    public GeminiClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _apiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"];
    }

    public async Task<string> GenerarJsonAsync(string prompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new ExternalServiceException("GEMINI_API_KEY no está configurada.");
        }

        var request = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { responseMimeType = "application/json", temperature = 0.3 }
        };

        var url = $"{BaseUrl}/models/{Modelo}:generateContent?key={Uri.EscapeDataString(_apiKey)}";

        using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        using var response = await _http.PostAsync(url, content, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"Gemini respondió {(int)response.StatusCode}: {Truncar(raw)}");
        }

        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            throw new ExternalServiceException("Gemini no devolvió candidatos.");
        }

        var texto = candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return texto ?? throw new ExternalServiceException("Gemini no devolvió contenido.");
    }

    private static string Truncar(string s, int max = 300)
    {
        return s.Length <= max ? s : s[..max];
    }
}
