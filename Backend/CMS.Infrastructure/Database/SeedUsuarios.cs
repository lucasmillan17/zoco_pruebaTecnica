using CMS.Domain;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Database
{
    /// <summary>
    /// Garantiza que existan los usuarios iniciales (admin/ventas).
    /// Idempotente: se ejecuta al arrancar la API y no repite registros.
    /// Ambos arrancan con DebeCambiarPassword = true para forzar el cambio
    /// de contraseña en el primer inicio de sesión.
    /// </summary>
    public static class SeedUsuarios
    {
        public static async Task EnsureAsync(CmsDbContext db)
        {
            if (await db.Usuarios.AnyAsync())
            {
                return;
            }

            var ahora = DateTime.UtcNow;

            db.Usuarios.AddRange(
                new Usuario
                {
                    NombreUsuario = "admin",
                    Nombre = "Administrador",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Rol = RolUsuario.Administrador,
                    Activo = true,
                    DebeCambiarPassword = true,
                    Email = "admin@cmszoco.local",
                    Telefono = null,
                    CreatedAt = ahora,
                    UpdatedAt = ahora
                },
                new Usuario
                {
                    NombreUsuario = "ventas",
                    Nombre = "Usuario de Ventas",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Ventas123!"),
                    Rol = RolUsuario.Ventas,
                    Activo = true,
                    DebeCambiarPassword = true,
                    Email = "ventas@cmszoco.local",
                    Telefono = null,
                    CreatedAt = ahora,
                    UpdatedAt = ahora
                });

            await db.SaveChangesAsync();
        }
    }
}
