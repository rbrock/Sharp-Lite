using System;
using System.Linq;
using System.Web.Mvc;
using SharpLite.Domain;
using SharpLite.Web.Annotations;

namespace SharpLite.Web.Mvc.ModelBinder
{
    /// <summary>
    /// Class EntityValueBinder.
    /// </summary>
    internal class EntityValueBinder : SharpModelBinder
    {
        /// <summary>
        /// Binds the model value to an entity by using the specified controller context and binding context.
        /// </summary>
        /// <param name="aControllerContext">The controller context.</param>
        /// <param name="aModelBindingContext">The model binding context.</param>
        /// <returns>The bound value.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aControllerContext or aModelBindingContext parameter is null.</exception>
        [CanBeNull]
        public override object BindModel([NotNull] ControllerContext aControllerContext, [NotNull] ModelBindingContext aModelBindingContext)
        {
            if (aControllerContext == null) throw new ArgumentNullException("aControllerContext");
            if (aModelBindingContext == null) throw new ArgumentNullException("aModelBindingContext");

            var lModelType = aModelBindingContext.ModelType;

            // Will look for the entity Id either named "ModelName" or "ModelName.Id"
            var lValueProviderResult =
                aModelBindingContext.ValueProvider.GetValue(aModelBindingContext.ModelName) ??
                aModelBindingContext.ValueProvider.GetValue(aModelBindingContext.ModelName + ".Id");

            if (lValueProviderResult != null)
            {
                var lEntityInterfaceType =
                    lModelType.GetInterfaces().First(
                        aInterfaceType =>
                            aInterfaceType.IsGenericType &&
                            aInterfaceType.GetGenericTypeDefinition() == typeof (IEntityWithTypedId<>));

                var lIDType = lEntityInterfaceType.GetGenericArguments().First();
                var lRawId = (lValueProviderResult.RawValue as string[]).First();

                if (string.IsNullOrEmpty(lRawId))
                {
                    return null;
                }

                try
                {
                    var lTypedId = (lIDType == typeof (Guid)) ? new Guid(lRawId) : Convert.ChangeType(lRawId, lIDType);
                    return EntityRetriever.GetEntityFor(lModelType, lTypedId, lIDType);
                }
                catch (Exception)
                {
                    // If the Id conversion failed for any reason, just return null
                    return null;
                }
            }

            return base.BindModel(aControllerContext, aModelBindingContext);
        }
    }
}