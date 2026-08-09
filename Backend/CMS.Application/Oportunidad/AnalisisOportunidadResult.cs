using System.Text.Json.Serialization;

namespace CMS.Application.Oportunidad;

/// <summary>
/// Resultado de la función "Analizar oportunidad".
/// </summary>
public record AnalisisOportunidadResult
{
    [JsonPropertyName("resumen")]
    public string Resumen { get; init; } = string.Empty;

    [JsonPropertyName("nivelInteres")]
    public string NivelInteres { get; init; } = "bajo";

    [JsonPropertyName("proximoPaso")]
    public string ProximoPaso { get; init; } = string.Empty;

    [JsonPropertyName("preguntas")]
    public List<string> Preguntas { get; init; } = new();

    [JsonPropertyName("datosFaltantes")]
    public List<string> DatosFaltantes { get; init; } = new();
}

public static class NivelInteres
{
    public const string Alto = "alto";
    public const string Medio = "medio";
    public const string Bajo = "bajo";
}
