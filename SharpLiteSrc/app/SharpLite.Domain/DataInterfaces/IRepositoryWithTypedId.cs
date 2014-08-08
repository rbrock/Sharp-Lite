using SharpLite.Domain.Annotations;
using System.Linq;

namespace SharpLite.Domain.DataInterfaces
{
    /// <summary>
    /// Provides a standard interface for Repositories which are data-access mechanism agnostic.
    /// Since nearly all of the domain objects you create will have a type of int Id, this
    /// base IRepository assumes that.  If you want an entity with a type
    /// other than int, such as string, then use <see cref="IRepositoryWithTypedId{TEntity, IdT}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    public interface IRepositoryWithTypedId<TEntity, in TId> where TEntity : class
    {
        /// <summary>
        /// Provides a handle to application wide DB activities such as committing any pending changes,
        /// beginning a transaction, rolling back a transaction, etc.
        /// </summary>
        /// <value>The database context.</value>
        [NotNull]
        IDbContext DbContext { get; }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="aEntity">The entity to delete.</param>
        /// <remarks>The SharpLite.NHibernateProvider.Repository commits the deletion immediately; see that class for details.</remarks>
        void Delete([NotNull] TEntity aEntity);

        /// <summary>
        /// Gets the entity with specified identifier or null if is not found.
        /// </summary>
        /// <param name="aID">The identifier.</param>
        /// <returns>The entity.</returns>
        [NotNull]
        TEntity Get(TId aID);

        /// <summary>
        /// Gets the collection of all entities.
        /// </summary>
        /// <returns>The collection of all entities</returns>
        [NotNull]
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// Either Save() or Update() the given instance, depending upon the value of its identifier property.
        /// </summary>
        /// <remarks>
        /// For entities with automatically generated Ids, such as identity or Hi/Lo, SaveOrUpdate may
        /// be called when saving or updating an entity.  If you require separate Save and Update
        /// methods, you'll need to extend the base repository interface when using NHibernate.
        /// Updating also allows you to commit changes to a detached object.  More info may be found at:
        /// http://www.hibernate.org/hib_docs/nhibernate/html_single/#manipulatingdata-updating-detached
        /// </remarks>
        /// <param name="aEntity">The entity to save or update.</param>
        /// <returns>The entity.</returns>
        [NotNull]
        TEntity SaveOrUpdate([NotNull] TEntity aEntity);
    }
}