using System.Linq.Expressions;
using CMS.Application.DBInterfaces;
using CMS.Application.DBInterfaces.Wrappers;
using CMS.Application.Exceptions;
using CMS.Domain;

namespace CMS.Application.Comercios;

public class ComercioService : IComercioService
{
    private static readonly IReadOnlyDictionary<EstadoComercio, HashSet<EstadoComercio>> Transiciones =
        new Dictionary<EstadoComercio, HashSet<EstadoComercio>>
        {
            [EstadoComercio.Nuevo] = new() { EstadoComercio.Contactado, EstadoComercio.Rechazado },
            [EstadoComercio.Contactado] = new() { EstadoComercio.Interesado, EstadoComercio.Rechazado },
            [EstadoComercio.Interesado] = new() { EstadoComercio.Documentacion, EstadoComercio.Rechazado },
            [EstadoComercio.Documentacion] = new() { EstadoComercio.Aprobado, EstadoComercio.Rechazado },
            [EstadoComercio.Aprobado] = new(),
            [EstadoComercio.Rechazado] = new()
        };

    private readonly IRepository _repo;

    public ComercioService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<ComercioDto>> GetAllAsync(BuscarComerciosQuery query)
    {
        var filtro = ConstruirFiltro(query);

        IOrderedQueryable<Comercio> Ordenar(IQueryable<Comercio> q)
        {
            var desc = query.Orden is null || query.Orden == OrdenDireccion.Desc;
            return (query.OrdenarPor, desc) switch
            {
                (OrdenComercio.RazonSocial, false) => q.OrderBy(c => c.RazonSocial),
                (OrdenComercio.RazonSocial, true) => q.OrderByDescending(c => c.RazonSocial),
                (OrdenComercio.Rubro, false) => q.OrderBy(c => c.Rubro == null).ThenBy(c => c.Rubro),
                (OrdenComercio.Rubro, true) => q.OrderBy(c => c.Rubro == null).ThenByDescending(c => c.Rubro),
                (OrdenComercio.Cuit, false) => q.OrderBy(c => c.Cuit),
                (OrdenComercio.Cuit, true) => q.OrderByDescending(c => c.Cuit),
                (OrdenComercio.Estado, false) => q.OrderBy(c => c.Estado),
                (OrdenComercio.Estado, true) => q.OrderByDescending(c => c.Estado),
                (OrdenComercio.FechaCreacion, false) => q.OrderBy(c => c.FechaDeCreacionEmpresa),
                (OrdenComercio.FechaCreacion, true) => q.OrderByDescending(c => c.FechaDeCreacionEmpresa),
                (OrdenComercio.UltimoContacto, false) => q.OrderBy(c => c.Interacciones.Max(i => i.FechaInteraccion) == null)
                    .ThenBy(c => c.Interacciones.Max(i => i.FechaInteraccion)),
                (OrdenComercio.UltimoContacto, true) => q.OrderBy(c => c.Interacciones.Max(i => i.FechaInteraccion) == null)
                    .ThenByDescending(c => c.Interacciones.Max(i => i.FechaInteraccion)),
                _ => q.OrderByDescending(c => c.CreatedAt)
            };
        }

        var page = await _repo.GetFiltered(filtro, query.PageNumber, query.PageSize, Ordenar);
        return new PagedResult<ComercioDto>
        {
            Items = page.Items.Select(Mapear).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }

    public async Task<ComercioDto?> GetByIdAsync(Guid id)
    {
        var comercio = await _repo.GetById<Comercio>(id);
        return comercio is null ? null : Mapear(comercio);
    }

    public async Task<ComercioDto> CreateAsync(CrearComercioDto dto)
    {
        if (!CuitValidator.EsValido(dto.Cuit))
        {
            throw new ConflictException("El CUIT no es válido (dígito verificador incorrecto).");
        }

        var existente = await _repo.First<Comercio>(c => c.Cuit == dto.Cuit);
        if (existente is not null)
        {
            throw new ConflictException($"Ya existe un comercio con el CUIT {dto.Cuit}.");
        }

        var comercio = new Comercio
        {
            RazonSocial = dto.RazonSocial.Trim(),
            Cuit = dto.Cuit,
            NombreDelContacto = dto.NombreDelContacto?.Trim(),
            Telefono = dto.Telefono?.Trim(),
            Direccion = dto.Direccion?.Trim(),
            Email = dto.Email?.Trim(),
            Rubro = dto.Rubro?.Trim(),
            Notas = dto.Notas?.Trim(),
            Estado = EstadoComercio.Nuevo,
            FechaDeCreacionEmpresa = DateTime.UtcNow,
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.Add(comercio);
        return Mapear(comercio);
    }

    public async Task<ComercioDto> UpdateAsync(Guid id, ActualizarComercioDto dto)
    {
        var comercio = await _repo.GetById<Comercio>(id)
            ?? throw new NotFoundException("Comercio no encontrado.");

        if (dto.Estado is not null && dto.Estado != comercio.Estado)
        {
            ValidarTransicion(comercio.Estado, dto.Estado.Value);
            comercio.Estado = dto.Estado.Value;
        }

        comercio.RazonSocial = dto.RazonSocial.Trim();
        if (dto.NombreDelContacto is not null) comercio.NombreDelContacto = dto.NombreDelContacto.Trim();
        if (dto.Telefono is not null) comercio.Telefono = dto.Telefono.Trim();
        if (dto.Direccion is not null) comercio.Direccion = dto.Direccion.Trim();
        if (dto.Email is not null) comercio.Email = dto.Email.Trim();
        if (dto.Rubro is not null) comercio.Rubro = dto.Rubro.Trim();
        if (dto.Notas is not null) comercio.Notas = dto.Notas.Trim();
        comercio.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(comercio);
        return Mapear(comercio);
    }

    /// <summary>
    /// Soft delete: marca Activo=false. Las interacciones se conservan.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var comercio = await _repo.GetById<Comercio>(id)
            ?? throw new NotFoundException("Comercio no encontrado.");

        comercio.Activo = false;
        comercio.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(comercio);
    }

    /// <summary>
    /// Reactivación explícita: Rechazado → Nuevo. No es una transición del Update.
    /// </summary>
    public async Task<ComercioDto> ReactivarAsync(Guid id)
    {
        var comercio = await _repo.GetById<Comercio>(id)
            ?? throw new NotFoundException("Comercio no encontrado.");

        if (comercio.Estado != EstadoComercio.Rechazado)
        {
            throw new ConflictException("Solo se puede reactivar un comercio en estado Rechazado.");
        }

        comercio.Estado = EstadoComercio.Nuevo;
        comercio.Activo = true;
        comercio.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(comercio);
        return Mapear(comercio);
    }

    private static void ValidarTransicion(EstadoComercio desde, EstadoComercio hacia)
    {
        if (!Transiciones.TryGetValue(desde, out var destinos) || !destinos.Contains(hacia))
        {
            throw new ConflictException($"Transición de estado inválida: {desde} → {hacia}.");
        }
    }

    private static Expression<Func<Comercio, bool>> ConstruirFiltro(BuscarComerciosQuery query)
    {
        Expression<Func<Comercio, bool>> filtro = c => true;

        if (!string.IsNullOrWhiteSpace(query.Busqueda))
        {
            var busqueda = query.Busqueda.Trim();
            filtro = Combinar(filtro, c =>
                c.RazonSocial.Contains(busqueda) ||
                c.Cuit.Contains(busqueda) ||
                (c.NombreDelContacto != null && c.NombreDelContacto.Contains(busqueda)) ||
                (c.Email != null && c.Email.Contains(busqueda)));
        }

        if (query.Estado is not null)
        {
            var estado = query.Estado.Value;
            filtro = Combinar(filtro, c => c.Estado == estado);
        }

        if (!string.IsNullOrWhiteSpace(query.Rubro))
        {
            var rubro = query.Rubro.Trim();
            filtro = Combinar(filtro, c => c.Rubro != null && c.Rubro.Contains(rubro));
        }

        return filtro;
    }

    private static Expression<Func<T, bool>> Combinar<T>(
        Expression<Func<T, bool>> izquierda,
        Expression<Func<T, bool>> derecha)
    {
        var parametro = izquierda.Parameters[0];
        var cuerpo = Expression.AndAlso(izquierda.Body, Expression.Invoke(derecha, parametro));
        return Expression.Lambda<Func<T, bool>>(cuerpo, parametro);
    }

    private static ComercioDto Mapear(Comercio c)
    {
        return new ComercioDto(
            c.Id,
            c.RazonSocial,
            c.Cuit,
            c.NombreDelContacto,
            c.Telefono,
            c.Direccion,
            c.Email,
            c.Rubro,
            c.FechaDeCreacionEmpresa,
            c.Notas,
            c.Estado,
            c.CreatedAt,
            c.UpdatedAt);
    }
}
