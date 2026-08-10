using System.Security.Claims;
using CMS.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var resultado = await _authService.LoginAsync(dto);
            return Ok(resultado);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _authService.MeAsync(usuarioId);
            return Ok(resultado);
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _authService.CambiarPasswordAsync(usuarioId, dto);
            return Ok(resultado);
        }

        [HttpGet("usuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListarUsuarios()
        {
            var resultado = await _authService.ListarUsuariosAsync();
            return Ok(resultado);
        }

        [HttpPost("usuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioDto dto)
        {
            var resultado = await _authService.CrearUsuarioAsync(dto);
            return CreatedAtAction(nameof(ListarUsuarios), null, resultado);
        }

        [HttpPost("usuarios/{id:guid}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarUsuario(Guid id)
        {
            var actorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _authService.DesactivarUsuarioAsync(actorId, id);
            return Ok(resultado);
        }
    }
}
