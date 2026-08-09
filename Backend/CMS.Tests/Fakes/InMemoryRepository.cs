using System.Linq.Expressions;
using CMS.Application.DBInterfaces;
using CMS.Application.DBInterfaces.Wrappers;
using CMS.Domain.Bases;

namespace CMS.Tests.Fakes;

/// <summary>
/// Repositorio en memoria para tests de unidad. No aplica query filters
/// (a diferencia de EF), lo cual es aceptable para probar la lógica de los services.
/// </summary>
public class InMemoryRepository : IRepository
{
    private readonly Dictionary<Type, List<EntityBase>> _store = new();

    public Task<T> Add<T>(T entity) where T : EntityBase
    {
        GetList<T>().Add(entity);
        return Task.FromResult(entity);
    }

    public Task<T> Update<T>(T entity) where T : EntityBase
    {
        var lista = GetList<T>();
        var indice = lista.FindIndex(e => e.Id == entity.Id);
        if (indice >= 0)
        {
            lista[indice] = entity;
        }
        return Task.FromResult(entity);
    }

    public Task<T> Delete<T>(T entity) where T : EntityBase
    {
        GetList<T>().RemoveAll(e => e.Id == entity.Id);
        return Task.FromResult(entity);
    }

    public Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase =>
        Task.FromResult(GetList<T>().FirstOrDefault(e => e.Id == id) as T);

    public Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase =>
        Task.FromResult(AsQuery<T>().FirstOrDefault(predicate.Compile()));

    public Task<List<T>> Find<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase =>
        Task.FromResult(AsQuery<T>().Where(predicate.Compile()).ToList());

    public Task<T?> Last<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase =>
        Task.FromResult(AsQuery<T>().Where(predicate.Compile()).OrderByDescending(e => e.CreatedAt).FirstOrDefault());

    public Task<PagedResult<T>> GetAll<T>(int pageNumber = 1, int pageSize = 10, params string[] include) where T : EntityBase =>
        GetFiltered<T>(_ => true, pageNumber, pageSize, null, include);

    public Task<PagedResult<T>> GetFiltered<T>(
        Expression<Func<T, bool>> predicate,
        int pageNumber = 1,
        int pageSize = 10,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params string[] include) where T : EntityBase
    {
        IQueryable<T> query = AsQuery<T>().Where(predicate);
        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        var total = query.Count();
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    private List<EntityBase> GetList<T>() where T : EntityBase
    {
        if (!_store.TryGetValue(typeof(T), out var lista))
        {
            lista = new List<EntityBase>();
            _store[typeof(T)] = lista;
        }
        return lista;
    }

    private IQueryable<T> AsQuery<T>() where T : EntityBase =>
        GetList<T>().Cast<T>().AsQueryable();
}
