namespace CMS.Application.Oportunidad;

/// <summary>
/// Contrato para invocar al proveedor de IA. La implementación (Gemini) vive en
/// CMS.Infrastructure. Devuelve el JSON crudo generado por el modelo.
/// </summary>
public interface IGeminiClient
{
    Task<string> GenerarJsonAsync(string prompt, CancellationToken ct = default);
}
