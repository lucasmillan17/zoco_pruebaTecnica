using System.Linq.Expressions;
using CMS.Application.DBInterfaces;
using CMS.Application.DBInterfaces.Wrappers;
using CMS.Application.Exceptions;
using CMS.Domain.Bases;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CMS.Infrastructure.Database
{
    /// <summary>
    /// Implementación del patrón Repository usando Entity Framework Core.
    /// Proporciona operaciones CRUD genéricas con soporte de paginación y eager loading.
    /// </summary>
    public class EfRepository : IRepository
    {
        private readonly CmsDbContext _context;

        /// <summary>
        /// Constructor que inicializa el repositorio con el contexto de base de datos.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework Core</param>
        public EfRepository(CmsDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<T> Add<T>(T entity) where T : EntityBase
        {
            await _context.AddAsync(entity);
            await GuardarAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<T> Delete<T>(T entity) where T : EntityBase
        {
            _context.Remove(entity);
            await GuardarAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
        {
            return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(predicate);
        }

        /// <inheritdoc/>
        public async Task<List<T>> Find<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
        {
            return await Include(_context.Set<T>(), include).Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Devuelve el ÚLTIMO registro que cumpla la condición, ordenando por fecha de creación
        /// de forma descendente. Ideal para buscar los registros más recientes.
        /// </summary>
        public async Task<T?> Last<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
        {
            return await Include(_context.Set<T>(), include)
                .Where(predicate)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<PagedResult<T>> GetAll<T>(int pageNumber = 1, int pageSize = 10, params string[] include) where T : EntityBase
        {
            var query = Include(_context.Set<T>(), include);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase
        {
            return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<T>> GetFiltered<T>(
            Expression<Func<T, bool>> predicate,
            int pageNumber = 1,
            int pageSize = 10,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params string[] include) where T : EntityBase
        {
            var query = Include(_context.Set<T>(), include).Where(predicate);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<T> Update<T>(T entity) where T : EntityBase
        {
            _context.Update(entity);
            await GuardarAsync();
            return entity;
        }

        /// <summary>
        /// Guarda los cambios traduciendo errores de base de datos a excepciones
        /// de dominio (ConflictException), para que la capa Application no dependa de EF.
        /// </summary>
        private async Task GuardarAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("El registro fue modificado por otro usuario. Recargá la información y volvé a intentar.");
            }
            catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
            {
                throw new ConflictException("Ya existe un registro con los mismos datos (restricción de unicidad).");
            }
        }

        private static bool EsViolacionDeUnicidad(DbUpdateException ex)
        {
            var actual = ex.InnerException;
            while (actual is not null)
            {
                if (actual is PostgresException pg && pg.SqlState == "23505")
                {
                    return true;
                }
                actual = actual.InnerException;
            }
            return false;
        }

        /// <summary>
        /// Aplica eager loading de propiedades de navegación especificadas.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <param name="query">Query base</param>
        /// <param name="includes">Array de nombres de propiedades a incluir</param>
        /// <returns>Query con includes aplicados</returns>
        private static IQueryable<T> Include<T>(IQueryable<T> query, string[] includes) where T : EntityBase
        {
            var includedQuery = query;

            foreach (var include in includes)
            {
                includedQuery = includedQuery.Include(include);
            }
            return includedQuery;
        }
    }
}
