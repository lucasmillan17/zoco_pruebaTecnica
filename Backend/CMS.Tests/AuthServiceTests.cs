using CMS.Application.Auth;
using CMS.Application.Exceptions;
using CMS.Domain;
using CMS.Tests.Fakes;

namespace CMS.Tests;

public class AuthServiceTests
{
    private static readonly JwtOptions Jwt = new(
        "TestIssuer",
        "TestAudience",
        "clave-de-prueba-para-tests-suficientemente-larga-1234567890",
        1);

    private static (AuthService service, InMemoryRepository repo) CrearService()
    {
        var repo = new InMemoryRepository();
        var service = new AuthService(repo, Jwt, new FakeCurrentUser());
        return (service, repo);
    }

    private static Task AgregarUsuario(InMemoryRepository repo, string nombreUsuario, string password, RolUsuario rol, bool activo = true, bool debeCambiarPassword = true)
    {
        return repo.Add(new Usuario
        {
            NombreUsuario = nombreUsuario,
            Nombre = "Usuario de prueba",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Rol = rol,
            Activo = activo,
            DebeCambiarPassword = debeCambiarPassword
        });
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveTokenYUsuario()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador);

        var resultado = await service.LoginAsync(new LoginDto("admin", "Admin123!"));

        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Equal("admin", resultado.Usuario.NombreUsuario);
        Assert.Equal(RolUsuario.Administrador, resultado.Usuario.Rol);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecto_LanzaUnauthorized()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(new LoginDto("admin", "mal-password")));
    }

    [Fact]
    public async Task Login_UsuarioInexistente_LanzaUnauthorized()
    {
        var (service, _) = CrearService();

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(new LoginDto("nadie", "Admin123!")));
    }

    [Fact]
    public async Task Login_UsuarioDesactivado_LanzaUnauthorized()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador, activo: false);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(new LoginDto("admin", "Admin123!")));
    }

    [Fact]
    public async Task Me_ConUsuarioExistente_DevuelveDatos()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "ventas", "Ventas123!", RolUsuario.Ventas);
        var creado = await repo.First<Usuario>(u => u.NombreUsuario == "ventas");

        var resultado = await service.MeAsync(creado.Id);

        Assert.Equal(creado.Id, resultado.Id);
        Assert.Equal("ventas", resultado.NombreUsuario);
        Assert.Equal(RolUsuario.Ventas, resultado.Rol);
    }

    [Fact]
    public async Task Me_UsuarioInexistente_LanzaUnauthorized()
    {
        var (service, _) = CrearService();

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.MeAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CrearUsuario_NormalizaNombreYHasheaPassword()
    {
        var (service, repo) = CrearService();

        var resultado = await service.CrearUsuarioAsync(new CrearUsuarioDto("  AdminDos  ", "Admin Dos", "Admin123!", RolUsuario.Administrador));

        Assert.Equal("admindos", resultado.NombreUsuario);
        Assert.Equal("admin", resultado.CreatedBy);
        var enRepo = await repo.First<Usuario>(u => u.NombreUsuario == "admindos");
        Assert.NotNull(enRepo);
        Assert.NotEqual("Admin123!", enRepo!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Admin123!", enRepo.PasswordHash));
    }

    [Fact]
    public async Task CrearUsuario_ConNombreDuplicado_LanzaConflict()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador);

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CrearUsuarioAsync(new CrearUsuarioDto("ADMIN", "Otro", "Admin123!", RolUsuario.Administrador)));
    }

    [Fact]
    public async Task CrearUsuario_Valido_ElUsuarioPuedeLoguearse()
    {
        var (service, repo) = CrearService();

        await service.CrearUsuarioAsync(new CrearUsuarioDto("ventas", "Ventas", "Ventas123!", RolUsuario.Ventas));

        var resultado = await service.LoginAsync(new LoginDto("ventas", "Ventas123!"));
        Assert.Equal(RolUsuario.Ventas, resultado.Usuario.Rol);
    }

    [Fact]
    public async Task Login_UsuarioNuevo_DevuelveFlagDebeCambiarPassword()
    {
        var (service, repo) = CrearService();
        await service.CrearUsuarioAsync(new CrearUsuarioDto("nuevo", "Nuevo", "Nueva123!", RolUsuario.Ventas, "nuevo@mail.com", "555-1234"));

        var resultado = await service.LoginAsync(new LoginDto("nuevo", "Nueva123!"));

        Assert.True(resultado.Usuario.DebeCambiarPassword);
        Assert.Equal("nuevo@mail.com", resultado.Usuario.Email);
    }

    [Fact]
    public async Task CambiarPassword_Valido_LimpiaFlagYActualizaHash()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "ventas", "Ventas123!", RolUsuario.Ventas);
        var usuario = await repo.First<Usuario>(u => u.NombreUsuario == "ventas");

        var resultado = await service.CambiarPasswordAsync(usuario!.Id, new CambiarPasswordDto("Ventas123!", "NuevaSegura123!"));

        Assert.False(resultado.DebeCambiarPassword);
        var enRepo = await repo.First<Usuario>(u => u.Id == usuario.Id);
        Assert.True(BCrypt.Net.BCrypt.Verify("NuevaSegura123!", enRepo!.PasswordHash));
    }

    [Fact]
    public async Task CambiarPassword_ConPasswordActualIncorrecta_LanzaUnauthorized()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "ventas", "Ventas123!", RolUsuario.Ventas);
        var usuario = await repo.First<Usuario>(u => u.NombreUsuario == "ventas");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.CambiarPasswordAsync(usuario!.Id, new CambiarPasswordDto("mal", "NuevaSegura123!")));
    }

    [Fact]
    public async Task DesactivarUsuario_PropiaCuenta_LanzaConflict()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador, debeCambiarPassword: false);
        var admin = await repo.First<Usuario>(u => u.NombreUsuario == "admin");

        await Assert.ThrowsAsync<ConflictException>(
            () => service.DesactivarUsuarioAsync(admin!.Id, admin.Id));
    }

    [Fact]
    public async Task DesactivarUsuario_OtroAdministrador_LanzaConflict()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador, debeCambiarPassword: false);
        await AgregarUsuario(repo, "admin2", "Admin123!", RolUsuario.Administrador, debeCambiarPassword: false);
        var admin = await repo.First<Usuario>(u => u.NombreUsuario == "admin");
        var admin2 = await repo.First<Usuario>(u => u.NombreUsuario == "admin2");

        await Assert.ThrowsAsync<ConflictException>(
            () => service.DesactivarUsuarioAsync(admin!.Id, admin2!.Id));
    }

    [Fact]
    public async Task DesactivarUsuario_DeVentas_Exitoso()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador, debeCambiarPassword: false);
        await AgregarUsuario(repo, "ventas", "Ventas123!", RolUsuario.Ventas, debeCambiarPassword: false);
        var admin = await repo.First<Usuario>(u => u.NombreUsuario == "admin");
        var ventas = await repo.First<Usuario>(u => u.NombreUsuario == "ventas");

        var resultado = await service.DesactivarUsuarioAsync(admin!.Id, ventas!.Id);

        Assert.False(resultado.Activo);
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(new LoginDto("ventas", "Ventas123!")));
    }

    [Fact]
    public async Task DesactivarUsuario_Inexistente_LanzaNotFound()
    {
        var (service, repo) = CrearService();
        await AgregarUsuario(repo, "admin", "Admin123!", RolUsuario.Administrador, debeCambiarPassword: false);
        var admin = await repo.First<Usuario>(u => u.NombreUsuario == "admin");

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DesactivarUsuarioAsync(admin!.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task ListarUsuarios_DevuelveTodosOrdenados()
    {
        var (service, repo) = CrearService();
        await service.CrearUsuarioAsync(new CrearUsuarioDto("admin", "Admin", "Admin123!", RolUsuario.Administrador));
        await service.CrearUsuarioAsync(new CrearUsuarioDto("ventas", "Ventas", "Ventas123!", RolUsuario.Ventas));
        var admin = await repo.First<Usuario>(u => u.NombreUsuario == "admin");
        var ventas = await repo.First<Usuario>(u => u.NombreUsuario == "ventas");
        await service.DesactivarUsuarioAsync(admin!.Id, ventas!.Id);

        var lista = await service.ListarUsuariosAsync();

        Assert.Equal(2, lista.Count);
        Assert.True(lista[0].Activo);
        Assert.Contains(lista, u => u.NombreUsuario == "ventas" && !u.Activo);
    }
}
