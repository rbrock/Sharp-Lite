using JetBrains.Annotations;

namespace SharpLite.Domain.DataInterfaces
{
    /// <summary>
    /// Interface IEntityDuplicateChecker
    /// </summary>
    public interface IEntityDuplicateChecker
    {
        /// <summary>
        /// Exist duplicate typed identifier of entity.
        /// </summary>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="aEntityWithTypedId">The entity.</param>
        /// <returns><c>true</c> if exist, <c>false</c> otherwise.</returns>
        bool DoesDuplicateExistWithTypedIdOf<TId>([NotNull] IEntityWithTypedId<TId> aEntityWithTypedId);
    }
}