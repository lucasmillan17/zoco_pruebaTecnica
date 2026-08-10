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

    [Fact]
    public async Task Reactivar_VuelveActivarElTipo()
    {
        var (service, repo) = CrearService();
        var creado = await service.CreateAsync(new CrearTipoInteraccionDto("demo", "Demo", null));
        await service.DeleteAsync(creado.Id);

        var resultado = await service.ReactivarAsync(creado.Id);

        Assert.True(resultado.Activo);
        var enRepo = await repo.GetById<TipoInteraccion>(creado.Id);
        Assert.True(enRepo!.Activo);
    }

    [Fact]
    public async Task GetAll_ActivosPorDefecto_NoIncluyeInactivos()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(new CrearTipoInteraccionDto("llamada", "Llamada", null));
        var demo = await service.CreateAsync(new CrearTipoInteraccionDto("demo", "Demo", null));
        await service.DeleteAsync(demo.Id);

        var resultado = await service.GetAllAsync();

        var unico = Assert.Single(resultado.Items);
        Assert.Equal("llamada", unico.Codigo);
    }

    [Fact]
    public async Task GetAll_Inactivos_SoloDevuelveDesactivados()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(new CrearTipoInteraccionDto("llamada", "Llamada", null));
        var demo = await service.CreateAsync(new CrearTipoInteraccionDto("demo", "Demo", null));
        await service.DeleteAsync(demo.Id);

        var resultado = await service.GetAllAsync(EstadoActivo.Inactivos);

        var unico = Assert.Single(resultado.Items);
        Assert.Equal("demo", unico.Codigo);
    }

    [Fact]
    public async Task GetAll_Todos_DevuelveActivosEInactivos()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(new CrearTipoInteraccionDto("llamada", "Llamada", null));
        var demo = await service.CreateAsync(new CrearTipoInteraccionDto("demo", "Demo", null));
        await service.DeleteAsync(demo.Id);

        var resultado = await service.GetAllAsync(EstadoActivo.Todos);

        Assert.Equal(2, resultado.Items.Count);
    }

    [Fact]
    public async Task Crear_ConCodigoDeTipoInactivo_LanzaConflict()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(new CrearTipoInteraccionDto("llamada", "Llamada", null));
        await service.DeleteAsync(creado.Id);

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(new CrearTipoInteraccionDto("llamada", "Otra Llamada", null)));
    }

    private static (TipoInteraccionService Service, InMemoryRepository Repo) CrearService()
    {
        var repo = new InMemoryRepository();
        return (new TipoInteraccionService(repo), repo);
    }
}
