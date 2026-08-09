using System.ComponentModel.DataAnnotations;
using CMS.Domain;

namespace CMS.Application.Comercios;

public enum OrdenComercio
{
    RazonSocial,
    Rubro,
    Cuit,
    Estado,
    FechaCreacion,
    UltimoContacto
}

public enum OrdenDireccion
{
    Asc,
    Desc
}

public record CrearComercioDto(
    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [MaxLength(200)]
    string RazonSocial,

    [Required(ErrorMessage = "El CUIT es obligatorio.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El CUIT debe tener 11 dígitos.")]
    string Cuit,

    [MaxLength(150)]
    string? NombreDelContacto,

    [MaxLength(50)]
    string? Telefono,

    [MaxLength(150)]
    string? Direccion,

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [MaxLength(150)]
    string? Email,

    [MaxLength(100)]
    string? Rubro,

    string? Notas);

/// <summary>
/// El CUIT es inmutable una vez creado (identificador del comercio).
/// </summary>
public record ActualizarComercioDto(
    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [MaxLength(200)]
    string RazonSocial,

    [MaxLength(150)]
    string? NombreDelContacto,

    [MaxLength(50)]
    string? Telefono,

    [MaxLength(150)]
    string? Direccion,

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [MaxLength(150)]
    string? Email,

    [MaxLength(100)]
    string? Rubro,

    string? Notas,

    EstadoComercio? Estado);

public record ComercioDto(
    Guid Id,
    string RazonSocial,
    string Cuit,
    string? NombreDelContacto,
    string? Telefono,
    string? Direccion,
    string? Email,
    string? Rubro,
    DateTime FechaDeCreacionEmpresa,
    string? Notas,
    EstadoComercio Estado,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record BuscarComerciosQuery(
    string? Busqueda = null,
    EstadoComercio? Estado = null,
    string? Rubro = null,
    OrdenComercio? OrdenarPor = null,
    OrdenDireccion? Orden = null,
    int PageNumber = 1,
    int PageSize = 10);
