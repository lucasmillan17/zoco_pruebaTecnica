using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.Domain.Bases
{
    public abstract class EntityBase
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Token de concurrencia optimista mapeado a la columna de sistema `xmin`
        /// de PostgreSQL (se renueva sola en cada UPDATE). Si dos usuarios intentan
        /// modificar el mismo registro, EF detecta el conflicto (DbUpdateConcurrencyException).
        /// </summary>
        public uint RowVersion { get; set; }
    }
}
