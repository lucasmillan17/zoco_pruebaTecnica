using CMS.Application.DBInterfaces;
using CMS.Application.DBInterfaces.Wrappers;
using CMS.Application.Exceptions;
using CMS.Domain;

namespace CMS.Application.Interacciones;

public class InteraccionService : IInteraccionService
{
    private readonly IRepository _repo;

    public InteraccionService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<InteraccionDto>> GetByComercioAsync(Guid comercioId, int pageNumber = 1, int pageSize = 10)
    {
        IOrderedQueryable<Interaccion> Ordenar(IQueryable<Interaccion> q) =>
            q.OrderByDescending(i => i.FechaInteraccion);

        var page = await _repo.GetFiltered(
            (Interaccion i) => i.ComercioId == comercioId,
            pageNumber,
            pageSize,
            Ordenar,
            "TipoInteraccion");

        return new PagedResult<InteraccionDto>
        {
            Items = page.Items.Select(Mapear).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }

    public async Task<InteraccionDto?> GetByIdAsync(Guid id)
    {
        var interaccion = await _repo.GetById<Interaccion>(id, "TipoInteraccion");
        return interaccion is null ? null : Mapear(interaccion);
    }

    public async Task<InteraccionDto> CreateAsync(CrearInteraccionDto dto)
    {
        var comercio = await _repo.GetById<Comercio>(dto.ComercioId)
            ?? throw new NotFoundException("Comercio no encontrado.");

        var tipo = await _repo.GetById<TipoInteraccion>(dto.TipoInteraccionId)
            ?? throw new NotFoundException("Tipo de interacción no encontrado o inactivo.");

        var interaccion = new Interaccion
        {
            ComercioId = dto.ComercioId,
            TipoInteraccionId = dto.TipoInteraccionId,
            FechaInteraccion = dto.FechaInteraccion ?? DateTime.UtcNow,
            Notas = dto.Notas?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.Add(interaccion);

        var conTipo = await _repo.GetById<Interaccion>(interaccion.Id, "TipoInteraccion");
        return Mapear(conTipo ?? interaccion);
    }

    public async Task<InteraccionDto> UpdateAsync(Guid id, ActualizarInteraccionDto dto)
    {
        var interaccion = await _repo.GetById<Interaccion>(id)
            ?? throw new NotFoundException("Interacción no encontrada.");

        if (dto.TipoInteraccionId is not null && dto.TipoInteraccionId != interaccion.TipoInteraccionId)
        {
            var tipo = await _repo.GetById<TipoInteraccion>(dto.TipoInteraccionId.Value)
                ?? throw new NotFoundException("Tipo de interacción no encontrado o inactivo.");
            interaccion.TipoInteraccionId = dto.TipoInteraccionId.Value;
        }

        if (dto.FechaInteraccion.HasValue)
        {
            interaccion.FechaInteraccion = dto.FechaInteraccion;
        }

        if (dto.Notas is not null) interaccion.Notas = dto.Notas.Trim();
        interaccion.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(interaccion);

        var conTipo = await _repo.GetById<Interaccion>(interaccion.Id, "TipoInteraccion");
        return Mapear(conTipo ?? interaccion);
    }

    public async Task DeleteAsync(Guid id)
    {
        var interaccion = await _repo.GetById<Interaccion>(id)
            ?? throw new NotFoundException("Interacción no encontrada.");

        await _repo.Delete(interaccion);
    }

    private static InteraccionDto Mapear(Interaccion i)
    {
        return new InteraccionDto(
            i.Id,
            i.ComercioId,
            i.TipoInteraccionId,
            i.TipoInteraccion?.Nombre,
            i.FechaInteraccion,
            i.Notas,
            i.CreatedAt);
    }
}
