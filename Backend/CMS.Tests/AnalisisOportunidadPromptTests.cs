using System.Reflection;
using CMS.Application.Oportunidad;
using CMS.Domain;

namespace CMS.Tests;

public class AnalisisOportunidadPromptTests
{
    [Fact]
    public void ConstruirPrompt_ContieneRubricaDePuntaje()
    {
        var prompt = ConstruirPromptDeContextoNuevo();

        Assert.Contains("score >= 5", prompt);
        Assert.Contains("score >= 2", prompt);
        Assert.Contains("Contactado +1", prompt);
        Assert.Contains("Interesado +3", prompt);
        Assert.Contains("Documentacion +3", prompt);
        Assert.Contains("Aprobado +4", prompt);
    }

    [Fact]
    public void ConstruirPrompt_ContieneReglasAntiOptimismo()
    {
        var prompt = ConstruirPromptDeContextoNuevo();

        Assert.Contains("nunca", prompt);
        Assert.Contains("Sin evidencia suficiente", prompt);
        Assert.Contains("comercio interesante para formar parte de zoco", prompt);
        Assert.Contains("REALISTA", prompt);
        Assert.Contains("CONSERVADOR", prompt);
    }

    [Fact]
    public void ConstruirPrompt_IncluyeDatosDelComercio()
    {
        var prompt = ConstruirPromptDeContextoNuevo();

        Assert.Contains("Gimnasio Maximus", prompt);
        Assert.Contains("Sin interacciones registradas.", prompt);
    }

    private static string ConstruirPromptDeContextoNuevo()
    {
        var ctx = new OportunidadContexto(
            "Gimnasio Maximus",
            "Gimnasio",
            EstadoComercio.Nuevo,
            "Comercio interesante para formar parte de zoco",
            new List<InteraccionContexto>());

        var metodo = typeof(AnalisisOportunidadService)
            .GetMethod("ConstruirPrompt", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("No se encontró el método ConstruirPrompt.");

        return (string)metodo.Invoke(null, new object[] { ctx })!;
    }
}
