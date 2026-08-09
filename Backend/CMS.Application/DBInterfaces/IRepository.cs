using System.Linq.Expressions;
using CMS.Application.DBInterfaces.Wrappers;
using CMS.Domain.Bases;

namespace CMS.Application.DBInterfaces
{
    /// <summary>
    /// Interfaz genérica del repositorio: operaciones CRUD, consultas con paginación
    /// y eager loading. Es el "port" de la arquitectura; la implementación EF vive en
    /// CMS.Infrastructure. No expone ningún tipo de Entity Framework.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Agrega una nueva entidad a la base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="entity">La entidad a agregar</param>
        /// <returns>La entidad agregada con su Id generado</returns>
        Task<T> Add<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Actualiza una entidad existente en la base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="entity">La entidad con los cambios a aplicar</param>
        /// <returns>La entidad actualizada</returns>
        Task<T> Update<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Elimina físicamente una entidad de la base de datos.
        /// Si la entidad usa soft delete (Activo), la regla de negocio vive en el service.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="entity">La entidad a eliminar</param>
        /// <returns>La entidad eliminada</returns>
        Task<T> Delete<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Obtiene una entidad por su identificador único.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="id">El identificador único de la entidad</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>La entidad encontrada o null si no existe</returns>
        Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene la primera entidad que cumple con el predicado especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="predicate">Expresión lambda que define el criterio de búsqueda</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>La primera entidad que cumple la condición o null si no existe</returns>
        Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene todas las entidades que cumplen el predicado (sin paginación).
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="predicate">Expresión lambda que define el criterio de filtrado</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>Lista de entidades que cumplen la condición</returns>
        Task<List<T>> Find<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene todas las entidades con soporte de paginación.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="pageNumber">Número de página (inicia en 1)</param>
        /// <param name="pageSize">Cantidad de elementos por página</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>Resultado paginado con los elementos y metadata de paginación</returns>
        Task<PagedResult<T>> GetAll<T>(int pageNumber = 1, int pageSize = 10, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene entidades filtradas con soporte de paginación y orden.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que hereda de EntityBase</typeparam>
        /// <param name="predicate">Expresión lambda que define el criterio de filtrado</param>
        /// <param name="pageNumber">Número de página (inicia en 1)</param>
        /// <param name="pageSize">Cantidad de elementos por página</param>
        /// <param name="orderBy">Función de ordenamiento sobre la query</param>
        /// <param name="include">Propiedades de navegación a incluir (eager loading)</param>
        /// <returns>Resultado paginado con los elementos filtrados y metadata de paginación</returns>
        Task<PagedResult<T>> GetFiltered<T>(
            Expression<Func<T, bool>> predicate,
            int pageNumber = 1,
            int pageSize = 10,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene el ÚLTIMO registro que cumpla la condición, ordenando por fecha de creación
        /// de forma descendente. Ideal para buscar los registros más recientes.
        /// </summary>
        Task<T?> Last<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;
    }
}
