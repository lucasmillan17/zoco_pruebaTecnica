using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMS.Application.DBInterfaces;
using CMS.Application.Exceptions;
using CMS.Domain;
using Microsoft.IdentityModel.Tokens;

namespace CMS.Application.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IRepository _repo;
        private readonly JwtOptions _jwt;
        private readonly ICurrentUser _currentUser;

        public AuthService(IRepository repo, JwtOptions jwt, ICurrentUser currentUser)
        {
            _repo = repo;
            _jwt = jwt;
            _currentUser = currentUser;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var nombreUsuario = dto.Usuario.Trim().ToLowerInvariant();
            var usuario = await _repo.First<Usuario>(u => u.NombreUsuario == nombreUsuario);

            if (usuario is null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedException("Usuario o contraseña incorrectos.");
            }

            return new LoginResponseDto(GenerarToken(usuario), Mapear(usuario));
        }

        public async Task<UsuarioDto> MeAsync(Guid usuarioId)
        {
            var usuario = await ObtenerUsuarioActivo(usuarioId);
            return Mapear(usuario);
        }

        public async Task<UsuarioDto> CambiarPasswordAsync(Guid usuarioId, CambiarPasswordDto dto)
        {
            var usuario = await ObtenerUsuarioActivo(usuarioId);

            if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
            {
                throw new UnauthorizedException("La contraseña actual es incorrecta.");
            }

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
            usuario.DebeCambiarPassword = false;
            usuario.UpdatedAt = DateTime.UtcNow;
            usuario.UpdatedBy = _currentUser.NombreUsuario;

            await _repo.Update(usuario);
            return Mapear(usuario);
        }

        public async Task<UsuarioDto> CrearUsuarioAsync(CrearUsuarioDto dto)
        {
            var nombreUsuario = dto.NombreUsuario.Trim().ToLowerInvariant();
            var existente = await _repo.First<Usuario>(u => u.NombreUsuario == nombreUsuario);
            if (existente is not null)
            {
                throw new ConflictException($"Ya existe un usuario con el nombre '{nombreUsuario}'.");
            }

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Nombre = dto.Nombre.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol,
                Activo = true,
                DebeCambiarPassword = true,
                Email = dto.Email?.Trim(),
                Telefono = dto.Telefono?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.NombreUsuario
            };

            await _repo.Add(usuario);
            return Mapear(usuario);
        }

        public async Task<List<UsuarioDto>> ListarUsuariosAsync()
        {
            var usuarios = await _repo.Find<Usuario>(_ => true);
            return usuarios
                .OrderByDescending(u => u.Activo)
                .ThenBy(u => u.Rol)
                .ThenBy(u => u.NombreUsuario)
                .Select(Mapear)
                .ToList();
        }

        public async Task<UsuarioDto> DesactivarUsuarioAsync(Guid actorId, Guid idObjetivo)
        {
            if (actorId == idObjetivo)
            {
                throw new ConflictException("No podés desactivar tu propia cuenta.");
            }

            var objetivo = await _repo.GetById<Usuario>(idObjetivo)
                ?? throw new NotFoundException("El usuario indicado no existe.");

            if (objetivo.Rol == RolUsuario.Administrador)
            {
                throw new ConflictException("No podés desactivar una cuenta de otro administrador.");
            }

            objetivo.Activo = false;
            objetivo.UpdatedAt = DateTime.UtcNow;
            objetivo.UpdatedBy = _currentUser.NombreUsuario;

            await _repo.Update(objetivo);
            return Mapear(objetivo);
        }

        private async Task<Usuario> ObtenerUsuarioActivo(Guid usuarioId)
        {
            var usuario = await _repo.GetById<Usuario>(usuarioId)
                ?? throw new UnauthorizedException("Sesión inválida.");

            if (!usuario.Activo)
            {
                throw new UnauthorizedException("El usuario fue desactivado.");
            }

            return usuario;
        }

        private string GenerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwt.ExpirationHours),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UsuarioDto Mapear(Usuario usuario)
            => new(
                usuario.Id,
                usuario.NombreUsuario,
                usuario.Nombre,
                usuario.Rol,
                usuario.Activo,
                usuario.DebeCambiarPassword,
                usuario.Email,
                usuario.Telefono,
                usuario.CreatedBy,
                usuario.UpdatedBy);
    }
}
