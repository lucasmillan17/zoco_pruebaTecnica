using System;
using System.Collections.Generic;
using System.Text;
using CMS.Domain.Bases;

namespace CMS.Domain
{
    public enum EstadoComercio
    {
        Nuevo,
        Contactado,
        Interesado,
        Documentacion,
        Aprobado,
        Rechazado
    }

    /// <summary>
    /// Filtro de visibilidad por estado Activo. Default: solo activos.
    /// </summary>
    public enum EstadoActivo
    {
        Activos,
        Inactivos,
        Todos
    }

    public class Comercio : EntityBase
    {
        public Comercio()
        {

        }
        public string RazonSocial { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public string? NombreDelContacto { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Email { get; set; }
        public string? Rubro { get; set; }
        public DateTime FechaDeCreacionEmpresa { get; set; }
        public string? Notas { get; set; }
        public EstadoComercio Estado { get; set; }
        public bool Activo { get; set; } = true;
        public List<Interaccion> Interacciones { get; set; } = new List<Interaccion>();
    }
}
