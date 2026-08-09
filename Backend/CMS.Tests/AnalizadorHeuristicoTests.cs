using CMS.Application.Oportunidad;
using CMS.Domain;

namespace CMS.Tests;

public class AnalizadorHeuristicoTests
{
    private readonly AnalizadorHeuristico _analizador = new();

    [Fact]
    public void Analizar_ComercioRechazado_SiempreDevuelveBajo()
    {
        var ctx = new OportunidadContexto(
            "Cafe",
            "Gastronomia",
            EstadoComercio.Rechazado,
            null,
            new List<InteraccionContexto>
            {
                new("demo", DateTime.UtcNow, "quiere presupuesto de pos")
            });

        var resultado = _analizador.Analizar(ctx);

        Assert.Equal(NivelInteres.Bajo, resultado.NivelInteres);
        Assert.Contains("Rechazado", resultado.Resumen);
    }

    [Fact]
    public void Analizar_ConInteresAlto_DevuelveAlto()
    {
        var ctx = new OportunidadContexto(
            "Cafe",
            "Gastronomia",
            EstadoComercio.Interesado,
            "Cliente con buena predisposicion",
            new List<InteraccionContexto>
            {
                new("reunion", DateTime.UtcNow, "quieren coordinar una demo del pos"),
                new("firma de contrato", DateTime.UtcNow, "presupuesto aprobado")
            });

        var resultado = _analizador.Analizar(ctx);

        Assert.Equal(NivelInteres.Alto, resultado.NivelInteres);
        Assert.NotEmpty(resultado.Preguntas);
    }

    [Fact]
    public void Analizar_ComercioNuevoSinDatos_DevuelveBajo()
    {
        var ctx = new OportunidadContexto(
            "Comercio Nuevo",
            "Kiosco",
            EstadoComercio.Nuevo,
            null,
            new List<InteraccionContexto>());

        var resultado = _analizador.Analizar(ctx);

        Assert.Equal(NivelInteres.Bajo, resultado.NivelInteres);
    }

    [Fact]
    public void Analizar_DetectaDatosFaltantes()
    {
        var ctx = new OportunidadContexto(
            "Cafe",
            "Gastronomia",
            EstadoComercio.Nuevo,
            null,
            new List<InteraccionContexto> { new("llamada", null, "solo una llamada breve") });

        var resultado = _analizador.Analizar(ctx);

        Assert.Contains(resultado.DatosFaltantes, d => d.Contains("volumen", StringComparison.OrdinalIgnoreCase));
    }
}
