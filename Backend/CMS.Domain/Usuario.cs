using CMS.Domain.Bases;

namespace CMS.Domain
{
    public enum RolUsuario
    {
        Administrador,
        Ventas
    }

    public class Usuario : EntityBase
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Obliga al usuario a cambiar su contraseña en el próximo inicio de sesión
        /// (se activa al crear/seedear la cuenta y se limpia tras el cambio).
        /// </summary>
        public bool DebeCambiarPassword { get; set; } = true;

        public string? Email { get; set; }
        public string? Telefono { get; set; }
    }
}
