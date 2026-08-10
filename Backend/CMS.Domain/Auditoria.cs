using CMS.Domain.Bases;

namespace CMS.Domain
{
    public enum OperacionAuditoria
    {
        Crear,
        Actualizar,
        Eliminar
    }

    /// <summary>
    /// Registro de auditoría generado automáticamente en SaveChangesAsync
    /// (una fila por campo modificado).
    /// </summary>
    public class Auditoria : EntityBase
    {
        /// <summary>Fecha y hora (UTC) en que se produjo el cambio.</summary>
        public DateTime Fecha { get; set; }

        /// <summary>Nombre de usuario que realizó el cambio ("sistema" si no hay sesión).</summary>
        public string? Usuario { get; set; }

        /// <summary>Rol del usuario que realizó el cambio.</summary>
        public string? Rol { get; set; }

        /// <summary>Nombre de la entidad modificada (ej: "Comercio").</summary>
        public string Entidad { get; set; } = string.Empty;

        /// <summary>Id del registro afectado.</summary>
        public Guid EntidadId { get; set; }

        /// <summary>Operación realizada: Crear, Actualizar o Eliminar (soft delete).</summary>
        public OperacionAuditoria Operacion { get; set; }

        /// <summary>Nombre del campo modificado.</summary>
        public string Campo { get; set; } = string.Empty;

        public string? ValorAnterior { get; set; }

        public string? ValorNuevo { get; set; }
    }
}
