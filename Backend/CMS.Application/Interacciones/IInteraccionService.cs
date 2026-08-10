using CMS.Application.DBInterfaces.Wrappers;

namespace CMS.Application.Interacciones;

public interface IInteraccionService
{
    Task<PagedResult<InteraccionDto>> GetByComercioAsync(
        Guid comercioId,
        Guid? tipoInteraccionId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int pageNumber = 1,
        int pageSize = 10);
    Task<InteraccionDto?> GetByIdAsync(Guid id);
    Task<InteraccionDto> CreateAsync(CrearInteraccionDto dto);
    Task<InteraccionDto> UpdateAsync(Guid id, ActualizarInteraccionDto dto);
    Task DeleteAsync(Guid id);
}
