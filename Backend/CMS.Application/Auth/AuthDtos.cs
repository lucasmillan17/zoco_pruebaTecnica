using System.ComponentModel.DataAnnotations;
using CMS.Domain;

namespace CMS.Application.Auth
{
    public record LoginDto(
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        string Usuario,

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        string Password);

    public record CrearUsuarioDto(
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [MaxLength(50)]
        string NombreUsuario,

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        string Nombre,

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        string Password,

        RolUsuario Rol,

        [MaxLength(150)]
        string? Email = null,

        [MaxLength(50)]
        string? Telefono = null);

    public record CambiarPasswordDto(
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        string PasswordActual,

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
        string PasswordNueva);

    public record UsuarioDto(
        Guid Id,
        string NombreUsuario,
        string Nombre,
        RolUsuario Rol,
        bool Activo,
        bool DebeCambiarPassword,
        string? Email,
        string? Telefono,
        string? CreatedBy,
        string? UpdatedBy);

    public record LoginResponseDto(string Token, UsuarioDto Usuario);
}
