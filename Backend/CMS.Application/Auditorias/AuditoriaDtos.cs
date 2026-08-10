using CMS.Domain;

namespace CMS.Application.Auditorias;

public record AuditoriaDto(
    Guid Id,
    DateTime Fecha,
    string? Usuario,
    string? Rol,
    string Entidad,
    Guid EntidadId,
    OperacionAuditoria Operacion,
    string Campo,
    string? ValorAnterior,
    string? ValorNuevo);

public record BuscarAuditoriaQuery(
    string? Entidad = null,
    string? Usuario = null,
    OperacionAuditoria? Operacion = null,
    DateTime? Desde = null,
    DateTime? Hasta = null,
    int PageNumber = 1,
    int PageSize = 20);
