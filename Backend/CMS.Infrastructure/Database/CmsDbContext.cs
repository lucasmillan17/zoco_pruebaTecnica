using System.Globalization;
using CMS.Application.Auth;
using CMS.Domain;
using CMS.Domain.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CMS.Infrastructure.Database
{
    public class CmsDbContext : DbContext
    {
        private readonly ICurrentUser _currentUser;

        public CmsDbContext(DbContextOptions<CmsDbContext> options, ICurrentUser currentUser) : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Comercio> Comercios => Set<Comercio>();
        public DbSet<Interaccion> Interacciones => Set<Interaccion>();
        public DbSet<TipoInteraccion> TiposInteraccion => Set<TipoInteraccion>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Auditoria> Auditorias => Set<Auditoria>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoInteraccion>(e =>
            {
                e.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
                e.Property(t => t.Codigo).IsRequired().HasMaxLength(50);
                e.Property(t => t.Descripcion).HasMaxLength(300);
                e.HasIndex(t => t.Codigo).IsUnique();
                e.HasData(Seed.TiposInteraccion);
            });

            modelBuilder.Entity<Comercio>(e =>
            {
                e.Property(c => c.RazonSocial).IsRequired().HasMaxLength(200);
                e.Property(c => c.Cuit).IsRequired().HasMaxLength(11);
                e.HasIndex(c => c.Cuit).IsUnique().HasFilter("\"Activo\"");
                e.Property(c => c.NombreDelContacto).HasMaxLength(150);
                e.Property(c => c.Telefono).HasMaxLength(50);
                e.Property(c => c.Email).HasMaxLength(150);
                e.Property(c => c.Rubro).HasMaxLength(100);
                e.Property(c => c.Notas).HasMaxLength(2000);
            });

            modelBuilder.Entity<Interaccion>(e =>
            {
                e.Property(i => i.Notas).HasMaxLength(2000);

                e.HasOne(i => i.Comercio)
                    .WithMany(c => c.Interacciones)
                    .HasForeignKey(i => i.ComercioId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(i => i.TipoInteraccion)
                    .WithMany()
                    .HasForeignKey(i => i.TipoInteraccionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Usuario>(e =>
            {
                e.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
                e.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
                e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
                e.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);
                e.Property(u => u.Email).HasMaxLength(150);
                e.Property(u => u.Telefono).HasMaxLength(50);
                e.HasIndex(u => u.NombreUsuario).IsUnique();
            });

            modelBuilder.Entity<Auditoria>(e =>
            {
                e.Property(a => a.Usuario).HasMaxLength(50);
                e.Property(a => a.Rol).HasMaxLength(20);
                e.Property(a => a.Entidad).IsRequired().HasMaxLength(50);
                e.Property(a => a.Campo).IsRequired().HasMaxLength(100);
                e.Property(a => a.ValorAnterior).HasMaxLength(500);
                e.Property(a => a.ValorNuevo).HasMaxLength(500);
                e.Property(a => a.Operacion).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(a => new { a.Entidad, a.EntidadId });
                e.HasIndex(a => a.Fecha);
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(EntityBase.RowVersion))
                        .IsRowVersion();
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(EntityBase.CreatedBy))
                        .HasMaxLength(50);
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(EntityBase.UpdatedBy))
                        .HasMaxLength(50);
                }
            }
        }

        /// <summary>
        /// Genera automáticamente los registros de auditoría antes de persistir los cambios:
        /// una fila por campo creado o modificado, con el usuario, rol y la operación realizada.
        /// La detección de "Eliminar" se hace vía soft delete (Activo pasa de true a false).
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditoria = ConstruirAuditoria();
            if (auditoria.Count > 0)
            {
                Auditorias.AddRange(auditoria);
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        private List<Auditoria> ConstruirAuditoria()
        {
            var ahora = DateTime.UtcNow;
            var usuario = _currentUser?.NombreUsuario ?? "sistema";
            var rol = _currentUser?.Rol;

            var excluidos = new HashSet<string>
            {
                nameof(EntityBase.Id),
                nameof(EntityBase.CreatedAt),
                nameof(EntityBase.UpdatedAt),
                nameof(EntityBase.CreatedBy),
                nameof(EntityBase.UpdatedBy),
                nameof(EntityBase.RowVersion),
                nameof(Usuario.PasswordHash)
            };

            var registros = new List<Auditoria>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Detached or EntityState.Unchanged)
                {
                    continue;
                }

                var tipo = entry.Metadata.ClrType;
                if (tipo == typeof(Auditoria) || !typeof(EntityBase).IsAssignableFrom(tipo))
                {
                    continue;
                }

                var entidadId = entry.Entity is EntityBase baseEntidad ? baseEntidad.Id : Guid.Empty;

                foreach (var prop in entry.Properties)
                {
                    if (excluidos.Contains(prop.Metadata.Name))
                    {
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                        {
                            var nuevo = Formatear(prop.CurrentValue);
                            if (nuevo is null) continue;
                            registros.Add(CrearRegistro(ahora, usuario, rol, tipo.Name, entidadId, OperacionAuditoria.Crear, prop.Metadata.Name, null, nuevo));
                            break;
                        }
                        case EntityState.Modified:
                        {
                            if (!prop.IsModified) continue;
                            var anterior = Formatear(prop.OriginalValue);
                            var nuevo = Formatear(prop.CurrentValue);
                            if (anterior == nuevo) continue;
                            var operacion = EsSoftDelete(prop) ? OperacionAuditoria.Eliminar : OperacionAuditoria.Actualizar;
                            registros.Add(CrearRegistro(ahora, usuario, rol, tipo.Name, entidadId, operacion, prop.Metadata.Name, anterior, nuevo));
                            break;
                        }
                        case EntityState.Deleted:
                        {
                            var anterior = Formatear(prop.OriginalValue);
                            if (anterior is null) continue;
                            registros.Add(CrearRegistro(ahora, usuario, rol, tipo.Name, entidadId, OperacionAuditoria.Eliminar, prop.Metadata.Name, anterior, null));
                            break;
                        }
                    }
                }
            }

            return registros;
        }

        private static Auditoria CrearRegistro(
            DateTime ahora,
            string usuario,
            string? rol,
            string entidad,
            Guid entidadId,
            OperacionAuditoria operacion,
            string campo,
            string? anterior,
            string? nuevo) => new()
        {
            Fecha = ahora,
            Usuario = usuario,
            Rol = rol,
            Entidad = entidad,
            EntidadId = entidadId,
            Operacion = operacion,
            Campo = campo,
            ValorAnterior = anterior,
            ValorNuevo = nuevo
        };

        private static bool EsSoftDelete(PropertyEntry prop) =>
            prop.Metadata.Name == nameof(Comercio.Activo)
            && prop.OriginalValue is bool anterior
            && prop.CurrentValue is bool actual
            && anterior && !actual;

        private static string? Formatear(object? valor)
        {
            if (valor is null) return null;
            if (valor is string s) return s.Length > 500 ? s[..500] : s;
            if (valor is DateTime dt) return dt.ToString("o", CultureInfo.InvariantCulture);
            if (valor is bool b) return b ? "true" : "false";
            return Convert.ToString(valor, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Datos iniciales de tablas catálogo.
    /// </summary>
    internal static class Seed
    {
        internal static readonly DateTime SeedDate = new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static readonly TipoInteraccion[] TiposInteraccion =
        {
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111101"), "llamada", "Llamada", "Llamada telefónica con el contacto."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111102"), "whatsapp", "WhatsApp", "Conversación por WhatsApp."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111103"), "reunion", "Reunión", "Reunión presencial o virtual con el comercio."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111104"), "email", "Email", "Correo electrónico enviado o recibido."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111105"), "nota_interna", "Nota interna", "Nota interna del equipo de ventas."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111106"), "demo", "Demo", "Demostración del producto (POS, QR, etc.)."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111107"), "visita", "Visita", "Visita presencial al local del comercio."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111108"), "videollamada", "Videollamada", "Videollamada (Meet, Zoom, etc.)."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-111111111109"), "envio_propuesta", "Envío de propuesta", "Envío de propuesta comercial o cotización."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-11111111110a"), "seguimiento", "Seguimiento", "Follow-up o recordatorio de contacto."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-11111111110b"), "queja", "Queja / problema", "Reclamo o problema reportado por el comercio."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-11111111110c"), "firma_contrato", "Firma de contrato", "Firma de contrato o acuerdo."),
            CrearTipo(Guid.Parse("11111111-1111-1111-1111-11111111110d"), "nota_sistema", "Nota de sistema", "Evento generado automáticamente por el sistema.")
        };

        private static TipoInteraccion CrearTipo(Guid id, string codigo, string nombre, string descripcion)
        {
            return new TipoInteraccion
            {
                Id = id,
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = descripcion,
                Activo = true,
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            };
        }
    }
}
