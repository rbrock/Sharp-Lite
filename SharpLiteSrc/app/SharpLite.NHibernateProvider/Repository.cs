using JetBrains.Annotations;
using NHibernate;
using SharpLite.Domain.DataInterfaces;

namespace SharpLite.NHibernateProvider
{
    /// <summary>
    /// Class Repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class Repository<TEntity> : RepositoryWithTypedId<TEntity, int>, IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="aSessionFactory">The session factory.</param>
        public Repository([NotNull] ISessionFactory aSessionFactory) : base(aSessionFactory)
        {
        }
    }
}
