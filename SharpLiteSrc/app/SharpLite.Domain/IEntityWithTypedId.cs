using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace SharpLite.Domain
{
    /// <summary>
    /// This serves as a base interface for <see cref="EntityWithTypedId{TId}" /> and
    /// <see cref="Entity" />. Also provides a simple means to develop your own base entity.
    /// </summary>
    /// <typeparam name="TId">The type of the t identifier.</typeparam>
    public interface IEntityWithTypedId<TId>
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        TId Id { get; set; }

        /// <summary>
        /// Gets the signature properties.
        /// </summary>
        /// <returns>The collection of property info.</returns>
        [NotNull]
        IEnumerable<PropertyInfo> GetSignatureProperties();

        /// <summary>
        /// Determines whether this instance is transient.
        /// </summary>
        /// <returns><c>true</c> if this instance is transient; otherwise, <c>false</c>.</returns>
        bool IsTransient();
    }
}