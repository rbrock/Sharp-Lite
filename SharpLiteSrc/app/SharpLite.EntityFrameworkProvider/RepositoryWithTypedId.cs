using System;
using System.Linq;
using SharpLite.Domain;
using SharpLite.Domain.DataInterfaces;
using SharpLite.EntityFrameworkProvider.Annotations;

namespace SharpLite.EntityFrameworkProvider
{
    /// <summary>
    /// Class RepositoryWithTypedId.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    public class RepositoryWithTypedId<TEntity, TId> : IRepositoryWithTypedId<TEntity, TId> where TEntity : class, IEntityWithTypedId<TId>
    {
        /// <summary>
        /// The database context
        /// </summary>
        private readonly System.Data.Entity.DbContext mDbContext;

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <value>The database context.</value>
        public virtual IDbContext DbContext {
            get {
                return new DbContext(mDbContext);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryWithTypedId{TEntity, TId}" /> class.
        /// </summary>
        /// <param name="aDbContext">The database context.</param>
        protected RepositoryWithTypedId([NotNull] System.Data.Entity.DbContext aDbContext)
        {
            mDbContext = aDbContext;
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="aEntity">The entity to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aEntity parameter is null.</exception>
        public virtual void Delete(TEntity aEntity)
        {
            if (aEntity == null) throw new ArgumentNullException("aEntity");

            mDbContext.Set<TEntity>().Remove(aEntity);
            mDbContext.SaveChanges();
        }

        /// <summary>
        /// Gets the entity with specified identifier or null if is not found.
        /// </summary>
        /// <param name="aID">The identifier.</param>
        /// <returns>The entity.</returns>
        public virtual TEntity Get(TId aID) {
            return mDbContext.Set<TEntity>().Single(aEntity => aID.Equals(aEntity.Id));
        }

        /// <summary>
        /// Gets the collection of all entities.
        /// </summary>
        /// <returns>The collection of all entities</returns>
        public virtual IQueryable<TEntity> GetAll() {
            return mDbContext.Set<TEntity>();
        }

        /// <summary>
        /// Either Save() or Update() the given instance, depending upon the value of its identifier property.
        /// </summary>
        /// <param name="aEntity">The entity to save or update.</param>
        /// <returns>The entity.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aEntity parameter is null.</exception>
        public virtual TEntity SaveOrUpdate(TEntity aEntity)
        {
            if (aEntity == null) throw new ArgumentNullException("aEntity");

            if (aEntity.IsTransient()) mDbContext.Set<TEntity>().Add(aEntity);

            return aEntity;
        }
    }
}