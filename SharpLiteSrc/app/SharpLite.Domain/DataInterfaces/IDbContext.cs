using System;
using JetBrains.Annotations;

namespace SharpLite.Domain.DataInterfaces
{
    /// <summary>
    /// Note that outside of CommitChanges(), you shouldn't have to invoke this object very often.
    /// If you're using the NHibernateSessionModule HttpModule, then the transaction
    /// opening/committing will be taken care of for you.
    /// </summary>
    public interface IDbContext
    {
        /// <summary>
        /// Begin a unit of work and return the associated ITransaction object.
        /// </summary>
        /// <returns>A transaction instance.</returns>
        [NotNull]
        IDisposable BeginTransaction();

        /// <summary>
        /// Synchronize the underlying persistent store with persistable state held in memory.
        /// </summary>
        void CommitChanges();

        /// <summary>
        /// Commit the underlying transaction if and only if the transaction was initiated by this object and end the unit of work.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rollbacks the underlying transaction if and only if the transaction was initiated by this object and end the unit of work.
        /// </summary>
        void RollbackTransaction();
    }
}