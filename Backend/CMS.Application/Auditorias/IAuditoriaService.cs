using CMS.Application.DBInterfaces.Wrappers;

namespace CMS.Application.Auditorias;

public interface IAuditoriaService
{
    Task<PagedResult<AuditoriaDto>> GetAllAsync(BuscarAuditoriaQuery query);
}
