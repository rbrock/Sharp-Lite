using System;
using System.Data;
using SharpLite.Domain.DataInterfaces;
using SharpLite.EntityFrameworkProvider.Annotations;

namespace SharpLite.EntityFrameworkProvider
{
    /// <summary>
    /// Class DbContext.
    /// </summary>
    public class DbContext : IDbContext
    {
        /// <summary>
        /// The database transaction
        /// </summary>
        private static IDbTransaction _transaction;

        /// <summary>
        /// The database context
        /// </summary>
        private readonly System.Data.Entity.DbContext mDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext" /> class.
        /// </summary>
        /// <param name="aDbContext">The database context.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aDbContext parameter is null.</exception>
        public DbContext([NotNull] System.Data.Entity.DbContext aDbContext)
        {
            if (aDbContext == null) throw new ArgumentNullException("aDbContext");

            mDbContext = aDbContext;
        }

        /// <summary>
        /// Begin a unit of work and return the associated ITransaction object.
        /// </summary>
        /// <returns>A transaction instance.</returns>
        public virtual IDisposable BeginTransaction() {
            _transaction = mDbContext.Database.Connection.BeginTransaction();

            return _transaction;
        }

        /// <summary>
        /// This isn't specific to any one DAO and flushes everything that has been
        /// changed since the last commit.
        /// </summary>
        public virtual void CommitChanges() {
            mDbContext.SaveChanges();
        }

        /// <summary>
        /// Commit the underlying transaction if and only if the transaction was initiated by this object and end the unit of work.
        /// </summary>
        public virtual void CommitTransaction() {
            if (_transaction != null)
                _transaction.Commit();
        }

        /// <summary>
        /// Rollbacks the underlying transaction if and only if the transaction was initiated by this object and end the unit of work.
        /// </summary>
        public virtual void RollbackTransaction() {
            if (_transaction != null)
                _transaction.Rollback();
        }
    }
}
