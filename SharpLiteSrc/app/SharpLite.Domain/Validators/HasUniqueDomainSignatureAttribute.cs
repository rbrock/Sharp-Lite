using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using JetBrains.Annotations;
using SharpLite.Domain.DataInterfaces;

// This is needed for the DependencyResolver...wish they would've just used Common Service Locator!

namespace SharpLite.Domain.Validators
{
    /// <summary>
    /// Due to the fact that .NET does not support generic attributes, this only works for entity
    /// types having an Id of type int.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    sealed public class HasUniqueDomainSignatureAttribute : ValidationAttribute
    {
        /// <summary>
        /// Determines whether the specified a value is valid.
        /// </summary>
        /// <param name="aValue">A value.</param>
        /// <param name="aValidationContext">A validation context.</param>
        /// <exception cref="System.InvalidOperationException">This validator must be used at the class level of an <see><cref>IEntityWithTypedId{int}</cref></see></exception>
        /// <exception cref="System.TypeLoadException"><see cref="IEntityDuplicateChecker" /> has not been registered with IoC</exception>
        [CanBeNull]
        protected override ValidationResult IsValid([CanBeNull] object aValue, [NotNull] ValidationContext aValidationContext)
        {
            if (aValue == null)
                return null;

            var lEntityToValidate = aValue as IEntityWithTypedId<int>;

            if (lEntityToValidate == null) throw new InvalidOperationException($"This validator must be used at the class level of an IEntityWithTypedId<int>. The type you provided was {aValue.GetType()}");

            var lDuplicateChecker = DependencyResolver.Current.GetService<IEntityDuplicateChecker>();

            if (lDuplicateChecker == null) throw new TypeLoadException("IEntityDuplicateChecker has not been registered with IoC");

            return lDuplicateChecker.DoesDuplicateExistWithTypedIdOf(lEntityToValidate)
                ? new ValidationResult(String.Empty)
                : null;
        }
    }
}
