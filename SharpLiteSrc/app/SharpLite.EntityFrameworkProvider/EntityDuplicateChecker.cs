using System;
using SharpLite.Domain;
using SharpLite.Domain.DataInterfaces;
using SharpLite.EntityFrameworkProvider.Annotations;

namespace SharpLite.EntityFrameworkProvider
{
    /// <summary>
    /// Class EntityDuplicateChecker.
    /// </summary>
    [UsedImplicitly]
    public class EntityDuplicateChecker : IEntityDuplicateChecker
    {
        /// <summary>
        /// Provides a behavior specific repository for checking if a duplicate exists of an existing entity.
        /// </summary>
        /// <typeparam name="TId">The type of the t identifier.</typeparam>
        /// <param name="aEntityWithTypedId">A entity with typed identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aEntityWithTypedId parameter is null.</exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool DoesDuplicateExistWithTypedIdOf<TId>(IEntityWithTypedId<TId> aEntityWithTypedId)
        {
            if (aEntityWithTypedId == null) throw new ArgumentNullException("aEntityWithTypedId");

            throw new NotImplementedException();
        }
    }
}
