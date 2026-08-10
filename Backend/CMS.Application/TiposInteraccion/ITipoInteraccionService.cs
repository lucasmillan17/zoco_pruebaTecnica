using CMS.Application.DBInterfaces.Wrappers;
using CMS.Domain;

namespace CMS.Application.TiposInteraccion;

public interface ITipoInteraccionService
{
    Task<PagedResult<TipoInteraccionDto>> GetAllAsync(EstadoActivo estadoActivo = EstadoActivo.Activos, int pageNumber = 1, int pageSize = 10);
    Task<TipoInteraccionDto?> GetByIdAsync(Guid id);
    Task<TipoInteraccionDto> CreateAsync(CrearTipoInteraccionDto dto);
    Task<TipoInteraccionDto> UpdateAsync(Guid id, ActualizarTipoInteraccionDto dto);
    Task DeleteAsync(Guid id);
    Task<TipoInteraccionDto> ReactivarAsync(Guid id);
}
