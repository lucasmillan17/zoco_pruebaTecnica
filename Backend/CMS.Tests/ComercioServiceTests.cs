using CMS.Application.Comercios;
using CMS.Application.Exceptions;
using CMS.Domain;
using CMS.Tests.Fakes;

namespace CMS.Tests;

public class ComercioServiceTests
{
    private const string CuitValido = "20123456786";

    [Fact]
    public async Task Crear_ConCuitInvalido_LanzaConflict()
    {
        var (service, _) = CrearService();

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(Dto("Cafe", "20123456785")));

        Assert.Contains("CUIT", ex.Message);
    }

    [Fact]
    public async Task ValidarCuit_CuitInvalido_DevuelveEsValidoFalse()
    {
        var (service, _) = CrearService();

        var resultado = await service.ValidarCuitAsync("20123456785");

        Assert.False(resultado.EsValido);
        Assert.False(resultado.Existe);
    }

    [Fact]
    public async Task ValidarCuit_ValidoNoExistente_DevuelveExisteFalse()
    {
        var (service, _) = CrearService();

        var resultado = await service.ValidarCuitAsync(CuitValido);

        Assert.True(resultado.EsValido);
        Assert.False(resultado.Existe);
    }

    [Fact]
    public async Task ValidarCuit_ValidoActivo_DevuelveExisteTrue()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(Dto("Cafe", CuitValido));

        var resultado = await service.ValidarCuitAsync(CuitValido);

        Assert.True(resultado.EsValido);
        Assert.True(resultado.Existe);
    }

    [Fact]
    public async Task ValidarCuit_ValidoPeroSoftDeleteado_DevuelveExisteFalse()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));
        await service.DeleteAsync(creado.Id);

        var resultado = await service.ValidarCuitAsync(CuitValido);

        Assert.True(resultado.EsValido);
        Assert.False(resultado.Existe);
    }

    [Fact]
    public async Task Crear_ConCuitValido_CreaConEstadoNuevo()
    {
        var (service, _) = CrearService();

        var resultado = await service.CreateAsync(Dto("Cafe La Esquina", CuitValido));

        Assert.Equal(EstadoComercio.Nuevo, resultado.Estado);
        Assert.Equal(CuitValido, resultado.Cuit);
        Assert.Equal("Cafe La Esquina", resultado.RazonSocial);
    }

    [Fact]
    public async Task Crear_RegistraCreadorEnAuditoria()
    {
        var (service, _) = CrearService();

        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));
        var actualizado = await service.UpdateAsync(creado.Id, UpdateDto("Cafe Renovado"));

        Assert.Equal("admin", creado.CreatedBy);
        Assert.Null(creado.UpdatedBy);
        Assert.Equal("admin", actualizado.UpdatedBy);
    }

    [Fact]
    public async Task Crear_ConCuitDuplicado_LanzaConflict()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(Dto("Cafe Uno", CuitValido));

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(Dto("Cafe Dos", CuitValido)));

        Assert.Contains("CUIT", ex.Message);
    }

    [Fact]
    public async Task Update_TransicionInvalida_LanzaConflict()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));

        // Nuevo → Aprobado no está permitido.
        await Assert.ThrowsAsync<ConflictException>(
            () => service.UpdateAsync(creado.Id, UpdateDto("Cafe", estado: EstadoComercio.Aprobado)));
    }

    [Fact]
    public async Task Update_TransicionValida_ActualizaEstado()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));

        var resultado = await service.UpdateAsync(creado.Id, UpdateDto("Cafe", estado: EstadoComercio.Contactado));

        Assert.Equal(EstadoComercio.Contactado, resultado.Estado);
    }

    [Fact]
    public async Task Update_Parcial_NoBorraCamposNoEnviados()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido, rubro: "Gastronomia"));

        // El PUT solo envía razonSocial y estado; el rubro no debe perderse.
        var resultado = await service.UpdateAsync(
            creado.Id,
            UpdateDto("Cafe Renombrado", estado: EstadoComercio.Contactado));

        Assert.Equal("Gastronomia", resultado.Rubro);
        Assert.Equal("Cafe Renombrado", resultado.RazonSocial);
    }

    [Fact]
    public async Task Delete_RealizaSoftDelete()
    {
        var (service, repo) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));

        await service.DeleteAsync(creado.Id);

        var enRepo = await repo.GetById<Comercio>(creado.Id);
        Assert.NotNull(enRepo);
        Assert.False(enRepo!.Activo);
    }

    [Fact]
    public async Task Reactivar_EnRechazado_CambiaANuevo()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));
        await service.UpdateAsync(creado.Id, UpdateDto("Cafe", estado: EstadoComercio.Rechazado));

        var resultado = await service.ReactivarAsync(creado.Id);

        Assert.Equal(EstadoComercio.Nuevo, resultado.Estado);
    }

    [Fact]
    public async Task Reactivar_EnOtroEstado_LanzaConflict()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));

        await Assert.ThrowsAsync<ConflictException>(() => service.ReactivarAsync(creado.Id));
    }

    [Fact]
    public async Task Reactivar_SoftDeleteadoEnOtroEstado_Recupera()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe", CuitValido));
        await service.DeleteAsync(creado.Id);

        var resultado = await service.ReactivarAsync(creado.Id);

        Assert.True(resultado.Activo);
        Assert.Equal(EstadoComercio.Nuevo, resultado.Estado);
    }

    [Fact]
    public async Task Crear_ConCuitDeComercioInactivo_NoLanzaConflict()
    {
        var (service, _) = CrearService();
        var creado = await service.CreateAsync(Dto("Cafe Uno", CuitValido));
        await service.DeleteAsync(creado.Id);

        var resultado = await service.CreateAsync(Dto("Cafe Dos", CuitValido));

        Assert.Equal(CuitValido, resultado.Cuit);
    }

    [Fact]
    public async Task GetAll_ActivosPorDefecto_NoIncluyeInactivos()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(Dto("Cafe Activo", CuitValido));
        var inactivo = await service.CreateAsync(Dto("Cafe Inactivo", "27123456780"));
        await service.DeleteAsync(inactivo.Id);

        var resultado = await service.GetAllAsync(new BuscarComerciosQuery());

        var unico = Assert.Single(resultado.Items);
        Assert.Equal("Cafe Activo", unico.RazonSocial);
    }

    [Fact]
    public async Task GetAll_Inactivos_SoloDevuelveDesactivados()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(Dto("Cafe Activo", CuitValido));
        var inactivo = await service.CreateAsync(Dto("Cafe Inactivo", "27123456780"));
        await service.DeleteAsync(inactivo.Id);

        var resultado = await service.GetAllAsync(new BuscarComerciosQuery(EstadoActivo: EstadoActivo.Inactivos));

        var unico = Assert.Single(resultado.Items);
        Assert.Equal("Cafe Inactivo", unico.RazonSocial);
        Assert.False(unico.Activo);
    }

    [Fact]
    public async Task GetAll_Todos_DevuelveActivosEInactivos()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(Dto("Cafe Activo", CuitValido));
        var inactivo = await service.CreateAsync(Dto("Cafe Inactivo", "27123456780"));
        await service.DeleteAsync(inactivo.Id);

        var resultado = await service.GetAllAsync(new BuscarComerciosQuery(EstadoActivo: EstadoActivo.Todos));

        Assert.Equal(2, resultado.Items.Count);
        Assert.Contains(resultado.Items, c => !c.Activo);
    }

    [Fact]
    public async Task GetAll_FiltraYOrdena()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(Dto("Zeta Cafe", "20123456786", rubro: "Gastronomia"));
        await service.CreateAsync(Dto("Alfa Bar", "27123456780", rubro: "Bar"));

        var query = new BuscarComerciosQuery(Rubro: "Gastronomia", OrdenarPor: OrdenComercio.RazonSocial, Orden: OrdenDireccion.Asc);
        var resultado = await service.GetAllAsync(query);

        var unico = Assert.Single(resultado.Items);
        Assert.Equal("Zeta Cafe", unico.RazonSocial);
    }

    [Fact]
    public async Task GetAll_OrdenaPorRubro()
    {
        var (service, _) = CrearService();
        await service.CreateAsync(Dto("Cafe B", "20123456786", rubro: "Bar"));
        await service.CreateAsync(Dto("Cafe A", "27123456780", rubro: "Gastronomia"));
        await service.CreateAsync(Dto("Sin Rubro", "30123456781"));

        var asc = await service.GetAllAsync(new BuscarComerciosQuery(OrdenarPor: OrdenComercio.Rubro, Orden: OrdenDireccion.Asc));
        Assert.Equal(new[] { "Cafe B", "Cafe A", "Sin Rubro" }, asc.Items.Select(i => i.RazonSocial).ToArray());

        var desc = await service.GetAllAsync(new BuscarComerciosQuery(OrdenarPor: OrdenComercio.Rubro, Orden: OrdenDireccion.Desc));
        Assert.Equal(new[] { "Cafe A", "Cafe B", "Sin Rubro" }, desc.Items.Select(i => i.RazonSocial).ToArray());
    }

    [Fact]
    public async Task GetAll_OrdenaPorUltimoContacto_SinContactoAlFinal()
    {
        var (service, repo) = CrearService();
        var nuevo = await service.CreateAsync(Dto("Sin Contacto", "20123456786"));
        var viejo = await service.CreateAsync(Dto("Contacto Viejo", "27123456780"));
        var reciente = await service.CreateAsync(Dto("Contacto Reciente", "30123456781"));

        var tipo = new TipoInteraccion
        {
            Id = Guid.NewGuid(),
            Codigo = "llamada",
            Nombre = "Llamada",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await repo.Add(tipo);

        await AgregarInteraccion(repo, viejo.Id, tipo, new DateTime(2026, 1, 10));
        await AgregarInteraccion(repo, reciente.Id, tipo, new DateTime(2026, 7, 20));

        var desc = await service.GetAllAsync(new BuscarComerciosQuery(OrdenarPor: OrdenComercio.UltimoContacto, Orden: OrdenDireccion.Desc));
        Assert.Equal(new[] { "Contacto Reciente", "Contacto Viejo", "Sin Contacto" }, desc.Items.Select(i => i.RazonSocial).ToArray());

        var asc = await service.GetAllAsync(new BuscarComerciosQuery(OrdenarPor: OrdenComercio.UltimoContacto, Orden: OrdenDireccion.Asc));
        Assert.Equal(new[] { "Contacto Viejo", "Contacto Reciente", "Sin Contacto" }, asc.Items.Select(i => i.RazonSocial).ToArray());
    }

    private static async Task AgregarInteraccion(InMemoryRepository repo, Guid comercioId, TipoInteraccion tipo, DateTime fecha)
    {
        var interaccion = new Interaccion
        {
            Id = Guid.NewGuid(),
            ComercioId = comercioId,
            TipoInteraccionId = tipo.Id,
            FechaInteraccion = fecha,
            CreatedAt = fecha,
            UpdatedAt = fecha
        };
        await repo.Add(interaccion);

        var comercio = await repo.GetById<Comercio>(comercioId);
        if (comercio is not null)
        {
            comercio.Interacciones.Add(interaccion);
        }
    }

    private static CrearComercioDto Dto(string razonSocial, string cuit, string? rubro = null) =>
        new(razonSocial, cuit, null, null, null, null, rubro, null);

    private static ActualizarComercioDto UpdateDto(string razonSocial, EstadoComercio? estado = null) =>
        new(razonSocial, null, null, null, null, null, null, estado);

    private static (ComercioService Service, InMemoryRepository Repo) CrearService()
    {
        var repo = new InMemoryRepository();
        return (new ComercioService(repo, new FakeCurrentUser()), repo);
    }
}
