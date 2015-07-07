using System;
using System.Reflection;
using System.Web.Mvc;
using JetBrains.Annotations;
using SharpLite.Domain.DataInterfaces;

namespace SharpLite.Web.Mvc.ModelBinder
{
    /// <summary>
    /// Class EntityRetriever.
    /// </summary>
    internal static class EntityRetriever
    {
        /// <summary>
        /// Gets the entity for the typed identifier value.
        /// </summary>
        /// <param name="aCollectionEntityType">Type of the collection entity.</param>
        /// <param name="aTypedIdValue">The typed identifier value.</param>
        /// <param name="aIDType">The identifier type.</param>
        /// <returns>System.Object.</returns>
        [NotNull]
        internal static object GetEntityFor([NotNull] Type aCollectionEntityType, [NotNull] object aTypedIdValue, [NotNull] Type aIDType)
        {
            var lEntityRepository = CreateEntityRepositoryFor(aCollectionEntityType, aIDType);

            return lEntityRepository.GetType().InvokeMember("Get", BindingFlags.InvokeMethod, null, lEntityRepository, new[] {aTypedIdValue});
        }

        /// <summary>
        /// Creates the entity repository for the entity.
        /// </summary>
        /// <param name="aEntityType">The entity type.</param>
        /// <param name="aIDType">The identifier type.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="System.TypeLoadException">Thrown if the entity type with identifier type has not been registered with IoC.</exception>
        [NotNull]
        private static object CreateEntityRepositoryFor([NotNull] Type aEntityType, [NotNull] Type aIDType)
        {
            var lConcreteRepositoryType = typeof (IRepositoryWithTypedId<,>).MakeGenericType(new[] {aEntityType, aIDType});

            var lRepository = DependencyResolver.Current.GetService(lConcreteRepositoryType);

            if (lRepository == null) throw new TypeLoadException(string.Format("{0} has not been registered with IoC", lConcreteRepositoryType));

            return lRepository;
        }
    }
}
