using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpLite.Domain.Annotations;

namespace SharpLite.Domain
{
    /// <summary>
    /// Provides a standard base class for facilitating comparison of objects.
    /// For a discussion of the implementation of Equals/GetHashCode, see
    /// http://devlicio.us/blogs/billy_mccafferty/archive/2007/04/25/using-equals-gethashcode-effectively.aspx
    /// and http://groups.google.com/group/sharp-architecture/browse_thread/thread/f76d1678e68e3ece?hl=en for
    /// an in depth and conclusive resolution.
    /// </summary>
    [Serializable]
    public abstract class ComparableObject
    {
        /// <summary>
        /// To help ensure hashcode uniqueness, a carefully selected random number multiplier
        /// is used within the calculation.  Goodrich and Tamassia's Data Structures and
        /// Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        /// of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        /// for more information.
        /// </summary>
        private const int lcHashMultiplier = 31;

        /// <summary>
        /// This static member caches the domain signature properties to avoid looking them up for
        /// each instance of the same type.
        /// A description of the very slick ThreadStatic attribute may be found at
        /// http://www.dotnetjunkies.com/WebLog/chris.taylor/archive/2005/08/18/132026.aspx
        /// </summary>
        [ThreadStatic]
        private static Dictionary<Type, IEnumerable<PropertyInfo>> _signaturePropertiesDictionary;

        /// <summary>
        /// This is used to provide the hashcode identifier of an object using the signature
        /// properties of the object; although it's necessary for NHibernate's use, this can
        /// also be useful for business logic purposes and has been included in this base
        /// class, accordingly.  Since it is recommended that GetHashCode change infrequently,
        /// if at all, in an object's lifetime, it's important that properties are carefully
        /// selected which truly represent the signature of an object.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() {
            unchecked {
                var lSignatureProperties = GetSignatureProperties().ToList();

                // It's possible for two objects to return the same hash code based on 
                // identically valued properties, even if they're of two different types, 
                // so we include the object's type in the hash calculation
                var lHashCode = GetType().GetHashCode();

                lHashCode = lSignatureProperties.Select(aProperty => aProperty.GetValue(this, null))
                    .Where(aValue => aValue != null)
                    .Aggregate(lHashCode, (aCurrent, aValue) => (aCurrent * lcHashMultiplier) ^ aValue.GetHashCode());

                // If no properties were flagged as being part of the signature of the object,
                // then simply return the hashcode of the base object as the hashcode.
                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                return lSignatureProperties.Any() ? lHashCode : base.GetHashCode();
            }
        }

        /// <summary>
        /// Gets the signature properties.
        /// </summary>
        /// <returns>The collection of property info.</returns>
        [NotNull]
        public virtual IEnumerable<PropertyInfo> GetSignatureProperties()
        {
            IEnumerable<PropertyInfo> lProperties;

            // Init the signaturePropertiesDictionary here due to reasons described at 
            // http://blogs.msdn.com/jfoscoding/archive/2006/07/18/670497.aspx
            if (_signaturePropertiesDictionary == null)
            {
                _signaturePropertiesDictionary = new Dictionary<Type, IEnumerable<PropertyInfo>>();
            }

            if (_signaturePropertiesDictionary.TryGetValue(GetType(), out lProperties))
            {
                return lProperties;
            }

            return _signaturePropertiesDictionary[GetType()] = GetTypeSpecificSignatureProperties();
        }

        /// <summary>
        /// Enforces the template method pattern to have child objects determine which specific
        /// properties should and should not be included in the object signature comparison. Note
        /// that the the BaseObject already takes care of performance caching, so this method
        /// shouldn't worry about caching...just return the goods man!
        /// </summary>
        /// <returns>The collection of property info.</returns>
        [NotNull]
        protected abstract IEnumerable<PropertyInfo> GetTypeSpecificSignatureProperties();

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="aComparableObject">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">aComparableObject</exception>
        public override bool Equals([CanBeNull] object aComparableObject)
        {
            if (aComparableObject == null) return false;

            var lCompareTo = aComparableObject as ComparableObject;

            if (ReferenceEquals(this, lCompareTo))
            {
                return true;
            }

            // ReSharper disable once CheckForReferenceEqualityInstead.1
            return lCompareTo != null && GetType().Equals(lCompareTo.GetTypeUnproxied()) && HasSameObjectSignatureAs(lCompareTo);
        }

        /// <summary>
        /// When NHibernate proxies objects, it masks the type of the actual entity object.
        /// This wrapper burrows into the proxied object to get its actual type.
        /// Although this assumes NHibernate is being used, it doesn't require any NHibernate
        /// related dependencies and has no bad side effects if NHibernate isn't being used.
        /// Related discussion is at http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
        /// </summary>
        /// <returns>The unproxied type.</returns>
        [NotNull]
        protected virtual Type GetTypeUnproxied()
        {
            return GetType();
        }

        /// <summary>
        /// You may override this method to provide your own comparison routine.
        /// </summary>
        /// <param name="aComparableObject">A comparable object.</param>
        /// <returns><c>true</c> if [has same object signature as] [the specified a comparable object]; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aComparableObject parameter is null.</exception>
        public virtual bool HasSameObjectSignatureAs([NotNull] ComparableObject aComparableObject)
        {
            if (aComparableObject == null) throw new ArgumentNullException("aComparableObject");

            var lSignatureProperties = GetSignatureProperties().ToList();

            if ((from lProperty in lSignatureProperties
                let lValueOfThisObject = lProperty.GetValue(this, null)
                let lValueToCompareTo = lProperty.GetValue(aComparableObject, null)
                where lValueOfThisObject != null || lValueToCompareTo != null
                where
                    (lValueOfThisObject == null ^ lValueToCompareTo == null) ||
                    (!lValueOfThisObject.Equals(lValueToCompareTo))
                select lValueOfThisObject).Any())
            {
                return false;
            }

            // If we've gotten this far and signature properties were found, then we can
            // assume that everything matched; otherwise, if there were no signature 
            // properties, then simply return the default bahavior of Equals
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            return lSignatureProperties.Any() || base.Equals(aComparableObject);
        }
    }
}