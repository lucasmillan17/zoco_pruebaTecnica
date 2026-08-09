using CMS.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.Domain
{
    public class TipoInteraccion : EntityBase
    {
        public TipoInteraccion() { }

        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
