using CMS.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.Domain
{
    public class Interaccion : EntityBase
    {
        public Interaccion() { }

        public Comercio Comercio { get; set; } = null!;
        public Guid ComercioId { get; set; }
        public DateTime? FechaInteraccion { get; set; }
        public TipoInteraccion TipoInteraccion { get; set; } = null!;
        public Guid TipoInteraccionId { get; set; }
        public string? Notas { get; set; }

    }
}
