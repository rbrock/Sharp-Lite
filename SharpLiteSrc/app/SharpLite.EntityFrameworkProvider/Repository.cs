using System;
using SharpLite.Domain;
using SharpLite.Domain.DataInterfaces;
using SharpLite.EntityFrameworkProvider.Annotations;

namespace SharpLite.EntityFrameworkProvider
{
    /// <summary>
    /// Class Repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class Repository<TEntity> : RepositoryWithTypedId<TEntity, int>, IRepository<TEntity> where TEntity : class, IEntityWithTypedId<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="aDbContext">The database context.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aDbContext parameter is null.</exception>
        public Repository([NotNull] System.Data.Entity.DbContext aDbContext)
            : base(aDbContext)
        {
            if (aDbContext == null) throw new ArgumentNullException("aDbContext");
        }
    }
}
