using CMS.Application.Auth;

namespace CMS.Tests.Fakes;

public class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(Guid? id = null, string? nombreUsuario = "admin", string? rol = "Administrador")
    {
        Id = id;
        NombreUsuario = nombreUsuario;
        Rol = rol;
    }

    public Guid? Id { get; }

    public string? NombreUsuario { get; }

    public string? Rol { get; }
}
