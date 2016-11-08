using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Criterion;
using SharpLite.Domain;
using SharpLite.Domain.DataInterfaces;

namespace SharpLite.NHibernateProvider
{
    /// <summary>
    /// Class EntityDuplicateChecker.
    /// </summary>
    public class EntityDuplicateChecker : IEntityDuplicateChecker
    {
        /// <summary>
        /// The session factory
        /// </summary>
        private readonly ISessionFactory mSessionFactory;

        /// <summary>
        /// The uninitialized datetime
        /// </summary>
        private readonly DateTime mUninitializedDatetime = default(DateTime);

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDuplicateChecker" /> class.
        /// </summary>
        /// <param name="aSessionFactory">The session factory.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aSessionFactory parameter is null.</exception>
        public EntityDuplicateChecker([NotNull] ISessionFactory aSessionFactory)
        {
            if (aSessionFactory == null) throw new ArgumentNullException(nameof(aSessionFactory));

            mSessionFactory = aSessionFactory;
        }

        /// <summary>
        /// Provides a behavior specific repository for checking if a duplicate exists of an existing entity.
        /// </summary>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="aEntityWithTypedId">The entity with typed identifier.</param>
        /// <returns><c>true</c> if exist duplicates, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Entity may not be null when checking for duplicates</exception>
        public bool DoesDuplicateExistWithTypedIdOf<TId>(IEntityWithTypedId<TId> aEntityWithTypedId) {
            if (aEntityWithTypedId == null) throw new ArgumentNullException(nameof(aEntityWithTypedId));

            var lSession = mSessionFactory.GetCurrentSession();
            var lPreviousFlushMode = lSession.FlushMode;

            // We do NOT want this to flush pending changes as checking for a duplicate should 
            // only compare the object against data that's already in the database
            lSession.FlushMode = FlushMode.Never;

            var lCriteria = lSession.CreateCriteria(aEntityWithTypedId.GetType()).Add(Restrictions.Not(Restrictions.Eq("Id", aEntityWithTypedId.Id))).SetMaxResults(1);

            AppendSignaturePropertyCriteriaTo(lCriteria, aEntityWithTypedId);
            var lDoesDuplicateExist = (lCriteria.List().Count > 0);
            lSession.FlushMode = lPreviousFlushMode;

            return lDoesDuplicateExist;
        }

        /// <summary>
        /// Appends the criteria to signature property.
        /// </summary>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="aCriteria">The criteria.</param>
        /// <param name="aEntityWithTypedId">The entity with typed identifier.</param>
        /// <exception cref="System.ApplicationException">Can't determine how to use  + entity.GetType() + . + signatureProperty.Name +
        /// when looking for duplicate entries. To remedy this,  +
        /// you can create a custom validator or report an issue to the S#arp Architecture  +
        /// project, detailing the type that you'd like to be accommodated.</exception>
        private void AppendSignaturePropertyCriteriaTo<TId>([NotNull] ICriteria aCriteria, [NotNull] IEntityWithTypedId<TId> aEntityWithTypedId)
        {
            foreach (var lSignatureProperty in aEntityWithTypedId.GetSignatureProperties())
            {
                var lPropertyType = lSignatureProperty.PropertyType;
                var lPropertyValue = lSignatureProperty.GetValue(aEntityWithTypedId, null);

                if (lPropertyType.IsEnum)
                {
                    aCriteria.Add(Restrictions.Eq(lSignatureProperty.Name, (int) lPropertyValue));
                }
                else if (
                    lPropertyType.GetInterfaces().Any(
                        aType => aType.IsGenericType && aType.GetGenericTypeDefinition() == typeof (IEntityWithTypedId<>)))
                {
                    AppendEntityCriteriaTo<TId>(aCriteria, lSignatureProperty, lPropertyValue);
                }
                else if (lPropertyType == typeof (DateTime))
                {
                    AppendDateTimePropertyCriteriaTo(aCriteria, lSignatureProperty, lPropertyValue);
                }
                else if (lPropertyType == typeof (string))
                {
                    AppendStringPropertyCriteriaTo(aCriteria, lSignatureProperty, lPropertyValue);
                }
                else if (lPropertyType.IsValueType)
                {
                    AppendValuePropertyCriteriaTo(aCriteria, lSignatureProperty, lPropertyValue);
                }
                else
                {
                    throw new ApplicationException(
                        "Can't determine how to use " + aEntityWithTypedId.GetType() + "." + lSignatureProperty.Name +
                        " when looking for duplicate entries. To remedy this, " +
                        "you can create a custom validator or report an issue to the S#arp Architecture " +
                        "project, detailing the type that you'd like to be accommodated.");
                }
            }
        }

        /// <summary>
        /// Appends the criteria to entity identifier.
        /// </summary>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="aCriteria">The criteria.</param>
        /// <param name="aSignatureProperty">The signature property.</param>
        /// <param name="aPropertyValue">The property value.</param>
        private static void AppendEntityCriteriaTo<TId>([NotNull] ICriteria aCriteria, [NotNull] PropertyInfo aSignatureProperty, [CanBeNull] object aPropertyValue)
        {
            aCriteria.Add(
                aPropertyValue != null
                    ? Restrictions.Eq(aSignatureProperty.Name + ".Id", ((IEntityWithTypedId<TId>) aPropertyValue).Id)
                    : Restrictions.IsNull(aSignatureProperty.Name + ".Id"));
        }

        /// <summary>
        /// Appends the criteria to date time property.
        /// </summary>
        /// <param name="aCriteria">The criteria.</param>
        /// <param name="aSignatureProperty">The signature property.</param>
        /// <param name="aPropertyValue">The property value.</param>
        private void AppendDateTimePropertyCriteriaTo([NotNull] ICriteria aCriteria, [NotNull] PropertyInfo aSignatureProperty, [NotNull] object aPropertyValue)
        {
            aCriteria.Add(
                (DateTime) aPropertyValue > mUninitializedDatetime
                    ? Restrictions.Eq(aSignatureProperty.Name, aPropertyValue)
                    : Restrictions.IsNull(aSignatureProperty.Name));
        }

        /// <summary>
        /// Appends the criteria to string property.
        /// </summary>
        /// <param name="aCriteria">The criteria.</param>
        /// <param name="aSignatureProperty">The signature property.</param>
        /// <param name="aPropertyValue">The property value.</param>
        private static void AppendStringPropertyCriteriaTo([NotNull] ICriteria aCriteria, [NotNull] PropertyInfo aSignatureProperty, [CanBeNull] object aPropertyValue)
        {
            aCriteria.Add(
                aPropertyValue != null
                    ? Restrictions.InsensitiveLike(aSignatureProperty.Name, aPropertyValue.ToString(), MatchMode.Exact)
                    : Restrictions.IsNull(aSignatureProperty.Name));
        }

        /// <summary>
        /// Appends the criteria to value property.
        /// </summary>
        /// <param name="aCriteria">The criteria.</param>
        /// <param name="aSignatureProperty">The signature property.</param>
        /// <param name="aPropertyValue">The property value.</param>
        private static void AppendValuePropertyCriteriaTo([NotNull] ICriteria aCriteria, [NotNull] PropertyInfo aSignatureProperty, [CanBeNull] object aPropertyValue)
        {
            aCriteria.Add(
                aPropertyValue != null
                    ? Restrictions.Eq(aSignatureProperty.Name, aPropertyValue)
                    : Restrictions.IsNull(aSignatureProperty.Name));
        }
    }
}
