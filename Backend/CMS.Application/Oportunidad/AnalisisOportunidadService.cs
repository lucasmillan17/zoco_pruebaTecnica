using System.Text.Json;
using CMS.Application.DBInterfaces;
using CMS.Application.Exceptions;
using CMS.Domain;

namespace CMS.Application.Oportunidad;

/// <summary>
/// Orquesta la función "Analizar oportunidad": carga el comercio y sus interacciones,
/// construye el prompt, invoca a Gemini y parsea el JSON. Si el proveedor de IA no está
/// configurado o falla, degrada a la heurística determinista.
/// </summary>
public class AnalisisOportunidadService : IAnalisisOportunidadService
{
    private readonly IRepository _repo;
    private readonly IGeminiClient _gemini;
    private readonly AnalizadorHeuristico _heuristica;

    public AnalisisOportunidadService(IRepository repo, IGeminiClient gemini, AnalizadorHeuristico heuristica)
    {
        _repo = repo;
        _gemini = gemini;
        _heuristica = heuristica;
    }

    public async Task<AnalisisOportunidadResult> AnalizarAsync(Guid comercioId, CancellationToken ct = default)
    {
        var comercio = await _repo.GetById<Comercio>(comercioId, "Interacciones", "Interacciones.TipoInteraccion")
            ?? throw new NotFoundException("Comercio no encontrado.");

        var interacciones = comercio.Interacciones
            .OrderBy(i => i.FechaInteraccion)
            .Select(i => new InteraccionContexto(i.TipoInteraccion?.Nombre ?? "desconocido", i.FechaInteraccion, i.Notas))
            .ToList();

        var contexto = new OportunidadContexto(comercio.RazonSocial, comercio.Rubro ?? "no informado", comercio.Estado, comercio.Notas, interacciones);

        try
        {
            var json = await _gemini.GenerarJsonAsync(ConstruirPrompt(contexto), ct);
            return ParseResultado(json);
        }
        catch (Exception ex) when (ex is ExternalServiceException or JsonException)
        {
            return _heuristica.Analizar(contexto);
        }
    }

    private static AnalisisOportunidadResult ParseResultado(string json)
    {
        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var resultado = JsonSerializer.Deserialize<AnalisisOportunidadResult>(json, opciones)
            ?? throw new ExternalServiceException("La respuesta de la IA fue vacía o inválida.");

        return resultado with { NivelInteres = NormalizarNivel(resultado.NivelInteres) };
    }

    private static string NormalizarNivel(string? nivel)
    {
        if (string.IsNullOrWhiteSpace(nivel))
        {
            return NivelInteres.Bajo;
        }

        var n = nivel.Trim().ToLowerInvariant();
        if (n.Contains("alt") || n is "high" or "alto" or "alta")
        {
            return NivelInteres.Alto;
        }
        if (n.Contains("med") || n is "medio" or "media" or "medium")
        {
            return NivelInteres.Medio;
        }
        return NivelInteres.Bajo;
    }

    private static string ConstruirPrompt(OportunidadContexto ctx)
    {
        var interaccionesTexto = ctx.Interacciones.Count == 0
            ? "Sin interacciones registradas."
            : string.Join("\n", ctx.Interacciones.Select(i =>
                $"- {i.Tipo} ({(i.Fecha?.ToString("yyyy-MM-dd") ?? "sin fecha")}): {i.Notas ?? "sin notas"}"));

        return $$"""
            Sos un analista comercial del equipo de ventas de ZOCO, empresa de soluciones de pago (POS y QR).
            Analizá la oportunidad de un comercio y respondé ÚNICAMENTE con JSON válido, sin texto adicional.

            Datos del comercio:
            - Nombre: {{ctx.RazonSocial}}
            - Rubro: {{ctx.Rubro ?? "no informado"}}
            - Estado en el pipeline: {{ctx.Estado}}
            - Notas del comercio: {{ctx.Notas ?? "sin notas"}}

            Interacciones registradas:
            {{interaccionesTexto}}

            Respondé con este esquema JSON exacto:
            {
              "resumen": "resumen ejecutivo del comercio (2-3 oraciones)",
              "nivelInteres": "alto" | "medio" | "bajo",
              "proximoPaso": "próxima acción recomendada para el vendedor",
              "preguntas": ["pregunta 1", "pregunta 2", "pregunta 3"],
              "datosFaltantes": ["dato faltante 1", "dato faltante 2"]
            }
            """;
    }
}
