using System.ComponentModel.DataAnnotations;

namespace CMS.Application.Interacciones;

public record CrearInteraccionDto(
    Guid ComercioId,

    Guid TipoInteraccionId,

    DateTime? FechaInteraccion,

    [MaxLength(2000)]
    string? Notas);

public record ActualizarInteraccionDto(
    Guid? TipoInteraccionId,

    DateTime? FechaInteraccion,

    [MaxLength(2000)]
    string? Notas);

public record InteraccionDto(
    Guid Id,
    Guid ComercioId,
    Guid TipoInteraccionId,
    string? TipoNombre,
    DateTime? FechaInteraccion,
    string? Notas,
    DateTime CreatedAt,
    string? CreatedBy,
    string? UpdatedBy);
