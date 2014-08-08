namespace SharpLite.Domain.DataInterfaces
{
    /// <summary>
    /// Provides a standard interface for Repositories which are data-access mechanism agnostic.
    /// Since nearly all of the domain objects you create will have a type of int Id, this
    /// base IRepository assumes that.  If you want an entity with a type
    /// other than int, such as string, then use <see cref="IRepositoryWithTypedId{TEntity, IdT}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity> : IRepositoryWithTypedId<TEntity, int> where TEntity : class
    {
    }
}