using CMS.Application.Exceptions;
using CMS.Application.Interacciones;
using CMS.Domain;
using CMS.Tests.Fakes;

namespace CMS.Tests;

public class InteraccionServiceTests
{
    private const string CuitValido = "20123456786";

    [Fact]
    public async Task Crear_ConComercioInexistente_LanzaNotFound()
    {
        var (service, _) = CrearService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(new CrearInteraccionDto(Guid.NewGuid(), Guid.NewGuid(), null, null)));
    }

    [Fact]
    public async Task Crear_ConTipoInexistente_LanzaNotFound()
    {
        var (service, repo) = CrearService();
        var comercio = new Comercio
        {
            RazonSocial = "Cafe",
            Cuit = CuitValido,
            Estado = EstadoComercio.Nuevo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await repo.Add(comercio);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(new CrearInteraccionDto(comercio.Id, Guid.NewGuid(), null, null)));
    }

    [Fact]
    public async Task Crear_Valido_UsaFechaActualSiNoSeProvee()
    {
        var (service, repo) = CrearService();
        var comercio = CrearComercioEnRepo(repo);
        var tipo = await repo.Add(new TipoInteraccion
        {
            Nombre = "Llamada",
            Codigo = "llamada",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var resultado = await service.CreateAsync(new CrearInteraccionDto(comercio.Id, tipo.Id, null, "Nota"));

        Assert.NotNull(resultado.FechaInteraccion);
        Assert.Equal("Nota", resultado.Notas);
    }

    [Fact]
    public async Task Crear_ConFechaUnspecified_GuardaComoUtc()
    {
        var (service, repo) = CrearService();
        var comercio = CrearComercioEnRepo(repo);
        var tipo = await repo.Add(new TipoInteraccion
        {
            Nombre = "Llamada",
            Codigo = "llamada",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var fecha = new DateTime(2026, 8, 9, 14, 30, 0, DateTimeKind.Unspecified);
        var resultado = await service.CreateAsync(new CrearInteraccionDto(comercio.Id, tipo.Id, fecha, null));

        Assert.Equal(DateTimeKind.Utc, resultado.FechaInteraccion!.Value.Kind);
        Assert.Equal(fecha.Ticks, resultado.FechaInteraccion.Value.Ticks);
    }

    [Fact]
    public async Task Update_ConFechaUnspecified_GuardaComoUtc()
    {
        var (service, repo) = CrearService();
        var comercio = CrearComercioEnRepo(repo);
        var tipo = await repo.Add(new TipoInteraccion
        {
            Nombre = "Llamada",
            Codigo = "llamada",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var creada = await service.CreateAsync(new CrearInteraccionDto(comercio.Id, tipo.Id, null, null));

        var fecha = new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Unspecified);
        var resultado = await service.UpdateAsync(creada.Id, new ActualizarInteraccionDto(null, fecha, null));

        Assert.Equal(DateTimeKind.Utc, resultado.FechaInteraccion!.Value.Kind);
        Assert.Equal(fecha.Ticks, resultado.FechaInteraccion.Value.Ticks);
    }

    [Fact]
    public async Task Update_NoBorraNotasSiNoSeEnvia()
    {
        var (service, repo) = CrearService();
        var comercio = CrearComercioEnRepo(repo);
        var tipo = await repo.Add(new TipoInteraccion
        {
            Nombre = "Llamada",
            Codigo = "llamada",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var creada = await service.CreateAsync(new CrearInteraccionDto(comercio.Id, tipo.Id, null, "Nota original"));

        var resultado = await service.UpdateAsync(creada.Id, new ActualizarInteraccionDto(null, null, null));

        Assert.Equal("Nota original", resultado.Notas);
    }

    [Fact]
    public async Task Delete_EliminaFisicamente()
    {
        var (service, repo) = CrearService();
        var comercio = CrearComercioEnRepo(repo);
        var tipo = await repo.Add(new TipoInteraccion
        {
            Nombre = "Llamada",
            Codigo = "llamada",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var creada = await service.CreateAsync(new CrearInteraccionDto(comercio.Id, tipo.Id, null, null));

        await service.DeleteAsync(creada.Id);

        var enRepo = await repo.GetById<Interaccion>(creada.Id);
        Assert.Null(enRepo);
    }

    private static (InteraccionService Service, InMemoryRepository Repo) CrearService()
    {
        var repo = new InMemoryRepository();
        return (new InteraccionService(repo), repo);
    }

    private static Comercio CrearComercioEnRepo(InMemoryRepository repo)
    {
        var comercio = new Comercio
        {
            RazonSocial = "Cafe",
            Cuit = CuitValido,
            Estado = EstadoComercio.Nuevo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        repo.Add(comercio).GetAwaiter().GetResult();
        return comercio;
    }
}
