using System.ComponentModel.DataAnnotations;

namespace CMS.Application.TiposInteraccion;

public record CrearTipoInteraccionDto(
    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(50)]
    string Codigo,

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    string Nombre,

    [MaxLength(300)]
    string? Descripcion);

/// <summary>
/// El código es inmutable una vez creado; solo se editan nombre y descripción.
/// </summary>
public record ActualizarTipoInteraccionDto(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    string Nombre,

    [MaxLength(300)]
    string? Descripcion);

public record TipoInteraccionDto(Guid Id, string Codigo, string Nombre, string? Descripcion, bool Activo);
