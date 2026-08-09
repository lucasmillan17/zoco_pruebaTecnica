using CMS.Domain;

namespace CMS.Application.Oportunidad;

public record InteraccionContexto(string Tipo, DateTime? Fecha, string? Notas);

public record OportunidadContexto(
    string RazonSocial,
    string Rubro,
    EstadoComercio Estado,
    string? Notas,
    List<InteraccionContexto> Interacciones);

/// <summary>
/// Fallback determinista de "Analizar oportunidad": reglas heurísticas que
/// funcionan sin API key ni red. Se usa cuando Gemini no está configurado o falla.
/// </summary>
public class AnalizadorHeuristico
{
    private static readonly string[] PalabrasInteres =
    {
        "interesad", "presupuest", "pos", "qr", "conciliacion", "transferencia",
        "efectivo", "quiero", "necesito", "cuando lo podemos", "mandame"
    };

    public AnalisisOportunidadResult Analizar(OportunidadContexto ctx)
    {
        var score = CalcularScore(ctx, out var recientes);
        var texto = TextoCompleto(ctx);

        var nivel = score >= 5 ? NivelInteres.Alto : score >= 2 ? NivelInteres.Medio : NivelInteres.Bajo;
        if (ctx.Estado == EstadoComercio.Rechazado)
        {
            nivel = NivelInteres.Bajo;
        }

        var datosFaltantes = DetectarDatosFaltantes(texto);
        var preguntas = ConstruirPreguntas(datosFaltantes, ctx.Rubro);

        return new AnalisisOportunidadResult
        {
            Resumen = ConstruirResumen(ctx, recientes),
            NivelInteres = nivel,
            ProximoPaso = ProximoPasoSegunEstado(ctx.Estado),
            Preguntas = preguntas,
            DatosFaltantes = datosFaltantes
        };
    }

    private static int CalcularScore(OportunidadContexto ctx, out int recientes)
    {
        var score = 0;

        switch (ctx.Estado)
        {
            case EstadoComercio.Contactado: score += 1; break;
            case EstadoComercio.Interesado: score += 3; break;
            case EstadoComercio.Documentacion: score += 3; break;
            case EstadoComercio.Aprobado: score += 4; break;
            case EstadoComercio.Rechazado: score -= 5; break;
        }

        var tipos = ctx.Interacciones.Select(i => i.Tipo.ToLowerInvariant()).ToHashSet();
        if (tipos.Contains("demo") || tipos.Contains("reunión") || tipos.Contains("firma de contrato"))
        {
            score += 3;
        }
        if (tipos.Contains("queja") || tipos.Contains("queja / problema"))
        {
            score -= 1;
        }

        var texto = TextoCompleto(ctx);
        score += PalabrasInteres.Count(p => texto.Contains(p));

        var umbral = DateTime.UtcNow.AddMonths(-1);
        recientes = ctx.Interacciones.Count(i => i.Fecha is not null && i.Fecha.Value >= umbral);
        if (recientes >= 3)
        {
            score += 2;
        }
        else if (recientes >= 1)
        {
            score += 1;
        }

        return score;
    }

    private static string TextoCompleto(OportunidadContexto ctx)
    {
        return string.Join(" ",
            ctx.Interacciones.Select(i => i.Notas ?? string.Empty)
                .Concat(new[] { ctx.Notas ?? string.Empty }))
            .ToLowerInvariant();
    }

    private static string ConstruirResumen(OportunidadContexto ctx, int recientes)
    {
        var resumen = $"Comercio de rubro {ctx.Rubro ?? "no informado"}, en estado {ctx.Estado}, con {ctx.Interacciones.Count} interacciones registradas.";

        if (!string.IsNullOrWhiteSpace(ctx.Notas))
        {
            var nota = ctx.Notas.Trim();
            resumen += $" Notas: {nota[..Math.Min(nota.Length, 200)]}";
        }
        if (recientes > 0)
        {
            resumen += $" Registró {recientes} interacciones en el último mes.";
        }

        return resumen;
    }

    private static string ProximoPasoSegunEstado(EstadoComercio estado)
    {
        return estado switch
        {
            EstadoComercio.Nuevo => "Realizar la primera llamada de contacto y presentar la propuesta.",
            EstadoComercio.Contactado => "Presentar la propuesta comercial y coordinar una demo.",
            EstadoComercio.Interesado => "Coordinar demo de POS + QR y detallar la solución de conciliación.",
            EstadoComercio.Documentacion => "Enviar la documentación y avanzar con la firma de contrato.",
            EstadoComercio.Aprobado => "Iniciar el onboarding y la activación del comercio.",
            EstadoComercio.Rechazado => "Replantear la propuesta o revincular el contacto más adelante.",
            _ => "Continuar el seguimiento comercial."
        };
    }

    private static List<string> DetectarDatosFaltantes(string texto)
    {
        var faltantes = new List<string>();
        if (!texto.Contains("volumen")) faltantes.Add("Volumen mensual aproximado");
        if (!texto.Contains("caja")) faltantes.Add("Cantidad de cajas / terminales");
        if (!texto.Contains("sucursal")) faltantes.Add("Cantidad de sucursales");
        if (!texto.Contains("cobro") && !texto.Contains("pago")) faltantes.Add("Métodos de cobro actuales");
        return faltantes;
    }

    private static List<string> ConstruirPreguntas(List<string> datosFaltantes, string? rubro)
    {
        var preguntas = new List<string>
        {
            $"¿Cuál es su volumen mensual de ventas aproximado ({rubro ?? "de su comercio"})?",
            "¿Cuántas cajas o terminales de cobro necesita operar?",
            "¿Con qué métodos de pago cobra hoy y tiene problemas de conciliación?"
        };

        if (datosFaltantes.Count == 0)
        {
            preguntas.Add("¿Cuándo podemos coordinar una demo de la solución?");
        }

        return preguntas.Take(3).ToList();
    }
}
