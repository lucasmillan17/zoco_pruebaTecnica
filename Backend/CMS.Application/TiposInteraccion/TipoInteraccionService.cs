using System.Text.RegularExpressions;
using CMS.Application.DBInterfaces;
using CMS.Application.DBInterfaces.Wrappers;
using CMS.Application.Exceptions;
using CMS.Domain;

namespace CMS.Application.TiposInteraccion;

public partial class TipoInteraccionService : ITipoInteraccionService
{
    private static readonly Regex CodigoValido = CodigoRegex();

    private readonly IRepository _repo;

    public TipoInteraccionService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<TipoInteraccionDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var page = await _repo.GetAll<TipoInteraccion>(pageNumber, pageSize);
        return new PagedResult<TipoInteraccionDto>
        {
            Items = page.Items.Select(Mapear).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }

    public async Task<TipoInteraccionDto?> GetByIdAsync(Guid id)
    {
        var tipo = await _repo.GetById<TipoInteraccion>(id);
        return tipo is null ? null : Mapear(tipo);
    }

    public async Task<TipoInteraccionDto> CreateAsync(CrearTipoInteraccionDto dto)
    {
        var codigo = NormalizarCodigo(dto.Codigo);

        var existente = await _repo.First<TipoInteraccion>(t => t.Codigo == codigo);
        if (existente is not null)
        {
            throw new ConflictException($"Ya existe un tipo de interacción con el código '{codigo}'.");
        }

        var entidad = new TipoInteraccion
        {
            Codigo = codigo,
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.Add(entidad);
        return Mapear(entidad);
    }

    public async Task<TipoInteraccionDto> UpdateAsync(Guid id, ActualizarTipoInteraccionDto dto)
    {
        var tipo = await _repo.GetById<TipoInteraccion>(id)
            ?? throw new NotFoundException("Tipo de interacción no encontrado.");

        tipo.Nombre = dto.Nombre.Trim();
        if (dto.Descripcion is not null) tipo.Descripcion = dto.Descripcion.Trim();
        tipo.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(tipo);
        return Mapear(tipo);
    }

    /// <summary>
    /// Soft delete: marca Activo=false. Las interacciones existentes conservan su tipo.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var tipo = await _repo.GetById<TipoInteraccion>(id)
            ?? throw new NotFoundException("Tipo de interacción no encontrado.");

        tipo.Activo = false;
        tipo.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(tipo);
    }

    private static string NormalizarCodigo(string codigo)
    {
        var normalizado = codigo.Trim().ToLowerInvariant();
        if (!CodigoValido.IsMatch(normalizado))
        {
            throw new ConflictException("El código solo puede contener letras minúsculas, números y guion bajo (ej: 'llamada', 'nota_interna').");
        }
        return normalizado;
    }

    private static TipoInteraccionDto Mapear(TipoInteraccion t)
    {
        return new TipoInteraccionDto(t.Id, t.Codigo, t.Nombre, t.Descripcion, t.Activo);
    }

    [GeneratedRegex(@"^[a-z][a-z0-9_]*$", RegexOptions.Compiled)]
    private static partial Regex CodigoRegex();
}
