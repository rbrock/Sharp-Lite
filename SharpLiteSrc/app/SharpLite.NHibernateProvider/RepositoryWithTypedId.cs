using System;
using System.Linq;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Linq;
using SharpLite.Domain.DataInterfaces;

namespace SharpLite.NHibernateProvider
{
    /// <summary>
    /// Class RepositoryWithTypedId.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    public class RepositoryWithTypedId<TEntity, TId> : IRepositoryWithTypedId<TEntity, TId> where TEntity : class
    {
        /// <summary>
        /// The session factory
        /// </summary>
        private readonly ISessionFactory mSessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryWithTypedId{TEntity, TId}" /> class.
        /// </summary>
        /// <param name="aSessionFactory">The session factory.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aSessionFactory parameter is null.</exception>
        public RepositoryWithTypedId([NotNull] ISessionFactory aSessionFactory)
        {
            if (aSessionFactory == null) throw new ArgumentNullException("aSessionFactory");

            mSessionFactory = aSessionFactory;
        }

        /// <summary>
        /// Provides a handle to application wide DB activities such as committing any pending changes,
        /// beginning a transaction, rolling back a transaction, etc.
        /// </summary>
        /// <value>The database context.</value>
        public virtual IDbContext DbContext {
            get {
                return new DbContext(mSessionFactory);
            }
        }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
        [NotNull]
        protected virtual ISession Session {
            get {
                return mSessionFactory.GetCurrentSession();
            }
        }

        /// <summary>
        /// This deletes the object and commits the deletion immediately.  We don't want to delay deletion
        /// until a transaction commits, as it may throw a foreign key constraint exception which we could
        /// likely handle and inform the user about.  Accordingly, this tries to delete right away; if there
        /// is a foreign key constraint preventing the deletion, an exception will be thrown.
        /// </summary>
        /// <param name="aEntity">The entity.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aEntity parameter is null.</exception>
        public virtual void Delete(TEntity aEntity)
        {
            if (aEntity == null) throw new ArgumentNullException("aEntity");

            Session.Delete(aEntity);
            Session.Flush();
        }

        /// <summary>
        /// Gets the entity with specified identifier or null if is not found.
        /// </summary>
        /// <param name="aID">The identifier.</param>
        /// <returns>The entity.</returns>
        public virtual TEntity Get(TId aID) {
            return Session.Get<TEntity>(aID);
        }

        /// <summary>
        /// Gets the collection of all entities.
        /// </summary>
        /// <returns>The collection of all entities</returns>
        public virtual IQueryable<TEntity> GetAll() {
            return Session.Query<TEntity>();
        }

        /// <summary>
        /// Either Save() or Update() the given instance, depending upon the value of its identifier property.
        /// </summary>
        /// <param name="aEntity">The entity to save or update.</param>
        /// <returns>The entity.</returns>
        public virtual TEntity SaveOrUpdate(TEntity aEntity) {
            if (aEntity == null) throw new ArgumentNullException("aEntity");

            Session.SaveOrUpdate(aEntity);
            return aEntity;
        }
    }
}