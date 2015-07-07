using System;
using JetBrains.Annotations;
using NHibernate;
using SharpLite.Domain.DataInterfaces;

namespace SharpLite.NHibernateProvider
{
    /// <summary>
    /// Class DbContext.
    /// </summary>
    public class DbContext : IDbContext
    {
        /// <summary>
        /// The _session factory
        /// </summary>
        private readonly ISessionFactory mSessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext" /> class.
        /// </summary>
        /// <param name="aSessionFactory">The session factory.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aSessionFactory parameter is null.</exception>
        public DbContext([NotNull] ISessionFactory aSessionFactory)
        {
            if (aSessionFactory == null) throw new ArgumentNullException("aSessionFactory");

            mSessionFactory = aSessionFactory;
        }

        /// <summary>
        /// Begin a unit of work and return the associated ITransaction object.
        /// </summary>
        /// <returns>A transaction instance.</returns>
        public virtual IDisposable BeginTransaction()
        {
            return mSessionFactory.GetCurrentSession().BeginTransaction();
        }

        /// <summary>
        /// Synchronize the underlying persistent store with persistable state held in memory.
        /// </summary>
        public virtual void CommitChanges()
        {
            mSessionFactory.GetCurrentSession().Flush();
        }

        /// <summary>
        /// Commit the underlying transaction if and only if the transaction was initiated by this object and end the unit of work.
        /// </summary>
        public virtual void CommitTransaction()
        {
            mSessionFactory.GetCurrentSession().Transaction.Commit();
        }

        /// <summary>
        /// Rollbacks the underlying transaction if and only if the transaction was initiated by this object and end the unit of work.
        /// </summary>
        public virtual void RollbackTransaction()
        {
            mSessionFactory.GetCurrentSession().Transaction.Rollback();
        }
    }
}