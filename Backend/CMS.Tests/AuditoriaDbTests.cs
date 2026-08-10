using System;
using System.Linq;
using System.Threading.Tasks;
using CMS.Domain;
using CMS.Infrastructure.Database;
using CMS.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace CMS.Tests;

public class AuditoriaDbTests
{
    [Fact]
    public async Task CrearComercio_RegistraUnaFilaPorCampoConUsuarioYRol()
    {
        using var db = CrearContext(new FakeCurrentUser(nombreUsuario: "admin", rol: "Administrador"));

        var comercio = NuevoComercio();
        db.Comercios.Add(comercio);
        await db.SaveChangesAsync();

        var filas = FilasDe(db, comercio).ToList();
        Assert.NotEmpty(filas);
        Assert.All(filas, f => Assert.Equal(OperacionAuditoria.Crear, f.Operacion));
        Assert.All(filas, f => Assert.Equal("admin", f.Usuario));
        Assert.All(filas, f => Assert.Equal("Administrador", f.Rol));
        Assert.Contains(filas, f => f.Campo == nameof(Comercio.RazonSocial) && f.ValorNuevo == "Zoco SA");
        Assert.Contains(filas, f => f.Campo == nameof(Comercio.Cuit) && f.ValorNuevo == "30500010012");
        Assert.All(filas, f => Assert.Null(f.ValorAnterior));
    }

    [Fact]
    public async Task ActualizarComercio_RegistraValoresAnteriorYNuevo()
    {
        using var db = CrearContext(new FakeCurrentUser());

        var comercio = NuevoComercio();
        db.Comercios.Add(comercio);
        await db.SaveChangesAsync();

        comercio.RazonSocial = "Zoco Renovada SA";
        comercio.Estado = EstadoComercio.Contactado;
        await db.SaveChangesAsync();

        var filaRazon = FilasDe(db, comercio).Last(f => f.Campo == nameof(Comercio.RazonSocial));
        Assert.Equal(OperacionAuditoria.Actualizar, filaRazon.Operacion);
        Assert.Equal("Zoco SA", filaRazon.ValorAnterior);
        Assert.Equal("Zoco Renovada SA", filaRazon.ValorNuevo);

        var filaEstado = FilasDe(db, comercio).Last(f => f.Campo == nameof(Comercio.Estado));
        Assert.Equal("Nuevo", filaEstado.ValorAnterior);
        Assert.Equal("Contactado", filaEstado.ValorNuevo);
    }

    [Fact]
    public async Task SoftDelete_RegistraOperacionEliminar()
    {
        using var db = CrearContext(new FakeCurrentUser());

        var comercio = NuevoComercio();
        db.Comercios.Add(comercio);
        await db.SaveChangesAsync();

        comercio.Activo = false;
        await db.SaveChangesAsync();

        var fila = FilasDe(db, comercio).Last(f => f.Campo == nameof(Comercio.Activo));
        Assert.Equal(OperacionAuditoria.Eliminar, fila.Operacion);
        Assert.Equal("true", fila.ValorAnterior);
        Assert.Equal("false", fila.ValorNuevo);
    }

    [Fact]
    public async Task SinUsuario_RegistraSistema()
    {
        using var db = CrearContext(new FakeCurrentUser(nombreUsuario: null, rol: null));

        var comercio = NuevoComercio();
        db.Comercios.Add(comercio);
        await db.SaveChangesAsync();

        var filas = FilasDe(db, comercio).ToList();
        Assert.NotEmpty(filas);
        Assert.All(filas, f => Assert.Equal("sistema", f.Usuario));
        Assert.All(filas, f => Assert.Null(f.Rol));
    }

    private static Comercio NuevoComercio() => new()
    {
        RazonSocial = "Zoco SA",
        Cuit = "30500010012",
        Estado = EstadoComercio.Nuevo,
        Activo = true,
        FechaDeCreacionEmpresa = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static IQueryable<Auditoria> FilasDe(CmsDbContext db, Comercio comercio) =>
        db.Auditorias.Where(a => a.Entidad == nameof(Comercio) && a.EntidadId == comercio.Id);

    private static CmsDbContext CrearContext(FakeCurrentUser user)
    {
        var options = new DbContextOptionsBuilder<CmsDbContext>()
            .UseInMemoryDatabase($"auditoria-{Guid.NewGuid()}")
            .Options;
        var db = new CmsDbContext(options, user);
        db.Database.EnsureCreated();
        return db;
    }
}
