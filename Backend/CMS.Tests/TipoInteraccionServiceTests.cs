using CMS.Application.Exceptions;
using CMS.Application.TiposInteraccion;
using CMS.Domain;
using CMS.Tests.Fakes;

namespace CMS.Tests;

public class TipoInteraccionServiceTests
{
    [Fact]
    public async Task Crear_ConCodigoDuplicado_LanzaConflict()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(new CrearTipoInteraccionDto("llamada", "Llamada", null));

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(new CrearTipoInteraccionDto("llamada", "Otra Llamada", null)));

        Assert.Contains("llamada", ex.Message);
    }

    [Fact]
    public async Task Crear_ConCodigoInvalido_LanzaConflict()
    {
        var (service, _) = CrearService();

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(new CrearTipoInteraccionDto("Llamada con espacios", "Llamada", null)));
    }

    [Fact]
    public async Task Crear_Valido_NormalizaCodigoAMinusculas()
    {
        var (service, _) = CrearService();

        var resultado = await service.CreateAsync(new CrearTipoInteraccionDto("  VISITA_Tienda  ", "Visita", null));

        Assert.Equal("visita_tienda", resultado.Codigo);
    }

    [Fact]
    public async Task Update_NoBorraDescripcionSiNoSeEnvia()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(new CrearTipoInteraccionDto("demo", "Demo", "Descripcion original"));

        var resultado = await service.UpdateAsync(creado.Id, new ActualizarTipoInteraccionDto("Demo Renombrada", null));

        Assert.Equal("Descripcion original", resultado.Descripcion);
        Assert.Equal("Demo Renombrada", resultado.Nombre);
    }

    [Fact]
    public async Task Delete_RealizaSoftDelete()
    {
        var (service, repo) = CrearService();
        var creado = await service.CreateAsync(new CrearTipoInteraccionDto("demo", "Demo", null));

        await service.DeleteAsync(creado.Id);

        var enRepo = await repo.GetById<TipoInteraccion>(creado.Id);
        Assert.NotNull(enRepo);
        Assert.False(enRepo!.Activo);
    }

    private static (TipoInteraccionService Service, InMemoryRepository Repo) CrearService()
    {
        var repo = new InMemoryRepository();
        return (new TipoInteraccionService(repo), repo);
    }
}
