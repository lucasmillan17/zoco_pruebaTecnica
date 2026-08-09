using CMS.Application.DBInterfaces.Wrappers;

namespace CMS.Application.TiposInteraccion;

public interface ITipoInteraccionService
{
    Task<PagedResult<TipoInteraccionDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<TipoInteraccionDto?> GetByIdAsync(Guid id);
    Task<TipoInteraccionDto> CreateAsync(CrearTipoInteraccionDto dto);
    Task<TipoInteraccionDto> UpdateAsync(Guid id, ActualizarTipoInteraccionDto dto);
    Task DeleteAsync(Guid id);
}
