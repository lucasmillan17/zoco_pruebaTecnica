using CMS.Application.DBInterfaces.Wrappers;

namespace CMS.Application.Comercios;

public interface IComercioService
{
    Task<PagedResult<ComercioDto>> GetAllAsync(BuscarComerciosQuery query);
    Task<ComercioDto?> GetByIdAsync(Guid id);
    Task<ComercioDto> CreateAsync(CrearComercioDto dto);
    Task<ComercioDto> UpdateAsync(Guid id, ActualizarComercioDto dto);
    Task DeleteAsync(Guid id);
    Task<ComercioDto> ReactivarAsync(Guid id);
}
