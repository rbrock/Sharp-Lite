using SharpLite.Domain;
using SharpLite.Web.Annotations;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SharpLite.Web.Mvc.ModelBinder
{
    /// <summary>
    /// Class EntityCollectionValueBinder.
    /// </summary>
    internal class EntityCollectionValueBinder : DefaultModelBinder
    {
        /// <summary>
        /// Binds the model to a value by using the specified controller context and binding context.
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

            var lCollectionType = aModelBindingContext.ModelType;
            var lCollectionEntityType = lCollectionType.GetGenericArguments().First();

            var lValueProviderResult = aModelBindingContext.ValueProvider.GetValue(aModelBindingContext.ModelName);

            if (lValueProviderResult != null)
            {
                var lRawValue = lValueProviderResult.RawValue as string[];

                if (lRawValue != null)
                {
                    var lCountOfEntityIds = lRawValue.Length;
                    var lEntities = Array.CreateInstance(lCollectionEntityType, lCountOfEntityIds);

                    var lEntityInterfaceType = lCollectionEntityType.GetInterfaces().First(aInterfaceType => aInterfaceType.IsGenericType && aInterfaceType.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));

                    var lIDType = lEntityInterfaceType.GetGenericArguments().First();

                    for (var lEntityIndex = 0; lEntityIndex < lCountOfEntityIds; lEntityIndex++)
                    {
                        var lRawId = lRawValue[lEntityIndex];

                        if (string.IsNullOrEmpty(lRawId))
                        {
                            return null;
                        }

                        var lTypedId = (lIDType == typeof(Guid)) ? new Guid(lRawId) : Convert.ChangeType(lRawId, lIDType);
                        var lEntity = EntityRetriever.GetEntityFor(lCollectionEntityType, lTypedId, lIDType);
                        lEntities.SetValue(lEntity, lEntityIndex);
                    }

                    return lEntities;
                }
            }

            return base.BindModel(aControllerContext, aModelBindingContext);
        }
    }
}