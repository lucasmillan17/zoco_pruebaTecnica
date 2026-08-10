using CMS.Application.DBInterfaces;
using CMS.Application.DBInterfaces.Wrappers;
using CMS.Domain;

namespace CMS.Application.Auditorias;

public class AuditoriaService : IAuditoriaService
{
    private readonly IRepository _repo;

    public AuditoriaService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<AuditoriaDto>> GetAllAsync(BuscarAuditoriaQuery query)
    {
        var page = await _repo.GetFiltered<Auditoria>(
            a =>
                (query.Entidad == null || a.Entidad == query.Entidad) &&
                (query.Usuario == null || (a.Usuario != null && a.Usuario.Contains(query.Usuario))) &&
                (query.Operacion == null || a.Operacion == query.Operacion) &&
                (query.Desde == null || a.Fecha >= query.Desde) &&
                (query.Hasta == null || a.Fecha <= query.Hasta),
            query.PageNumber,
            query.PageSize,
            q => q.OrderByDescending(a => a.Fecha));

        return new PagedResult<AuditoriaDto>
        {
            Items = page.Items.Select(Mapear).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }

    private static AuditoriaDto Mapear(Auditoria a) => new(
        a.Id,
        a.Fecha,
        a.Usuario,
        a.Rol,
        a.Entidad,
        a.EntidadId,
        a.Operacion,
        a.Campo,
        a.ValorAnterior,
        a.ValorNuevo);
}
