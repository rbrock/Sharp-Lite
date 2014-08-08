
using SharpLite.EntityFrameworkProvider.Annotations;

namespace SharpLite.EntityFrameworkProvider
{
    /// <summary>
    /// Class EntityFrameworkQuery.
    /// </summary>
    public abstract class EntityFrameworkQuery
    {
        /// <summary>
        /// The _DB context
        /// </summary>
        private readonly System.Data.Entity.DbContext mDbContext;

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <value>The database context.</value>
        [NotNull]
        protected System.Data.Entity.DbContext DbContext {
            get {
                return mDbContext;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkQuery" /> class.
        /// </summary>
        /// <param name="aDbContext">The database context.</param>
        protected EntityFrameworkQuery([NotNull] System.Data.Entity.DbContext aDbContext)
        {
            mDbContext = aDbContext;
        }
    }
}