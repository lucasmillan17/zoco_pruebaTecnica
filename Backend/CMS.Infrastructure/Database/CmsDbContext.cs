using CMS.Domain;
using CMS.Domain.Bases;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Database
{
    public class CmsDbContext : DbContext
    {
        public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options)
        {
        }

        public DbSet<Comercio> Comercios => Set<Comercio>();
        public DbSet<Interaccion> Interacciones => Set<Interaccion>();
        public DbSet<TipoInteraccion> TiposInteraccion => Set<TipoInteraccion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoInteraccion>(e =>
            {
                e.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
                e.Property(t => t.Codigo).IsRequired().HasMaxLength(50);
                e.Property(t => t.Descripcion).HasMaxLength(300);
                e.HasIndex(t => t.Codigo).IsUnique();
                e.HasQueryFilter(t => t.Activo);
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
                e.HasQueryFilter(c => c.Activo);
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

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(EntityBase.RowVersion))
                        .IsRowVersion();
                }
            }
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
