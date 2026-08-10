namespace CMS.Application.Auth;

/// <summary>
/// Información del usuario autenticado (leída del JWT) para registrar auditoría.
/// </summary>
public interface ICurrentUser
{
    Guid? Id { get; }

    string? NombreUsuario { get; }

    string? Rol { get; }
}
