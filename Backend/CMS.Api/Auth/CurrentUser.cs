using System.Security.Claims;
using CMS.Application.Auth;

namespace CMS.Api.Auth;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? Id
    {
        get
        {
            var valor = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public string? NombreUsuario =>
        _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public string? Rol =>
        _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
}
