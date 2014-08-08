using SharpLite.Domain;
using SharpLite.Web.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace SharpLite.Web.Mvc.ModelBinder
{
    /// <summary>
    /// Class SharpModelBinder.
    /// </summary>
    public class SharpModelBinder : DefaultModelBinder
    {
        /// <summary>
        /// The local constant for identifier property name
        /// </summary>
        private const string lcIdPropertyName = "Id";

        //public static object IEntityWithTypedId { get; set; } //UNUSED PROP

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="aControllerContext">A controller context.</param>
        /// <param name="aBindingContext">A binding context.</param>
        /// <param name="aPropertyDescriptor">A property descriptor.</param>
        /// <param name="aPropertyBinder">A property binder.</param>
        /// <returns>System.Object.</returns>
        [NotNull]
        protected override object GetPropertyValue([NotNull] ControllerContext aControllerContext, [NotNull] ModelBindingContext aBindingContext, [NotNull] PropertyDescriptor aPropertyDescriptor, [NotNull] IModelBinder aPropertyBinder)
        {
            var lPropertyType = aPropertyDescriptor.PropertyType;

            if (IsEntityType(lPropertyType))
            {
                // use the EntityValueBinder
                return base.GetPropertyValue(aControllerContext, aBindingContext, aPropertyDescriptor, new EntityValueBinder());
            }

            if (IsSimpleGenericBindableEntityCollection(lPropertyType))
            {
                // use the EntityValueCollectionBinder
                return base.GetPropertyValue(aControllerContext, aBindingContext, aPropertyDescriptor, new EntityCollectionValueBinder());
            }

            return base.GetPropertyValue(aControllerContext, aBindingContext, aPropertyDescriptor, aPropertyBinder);
        }

        /// <summary>
        /// Determines whether [is entity type] [the specified a property type].
        /// </summary>
        /// <param name="aPropertyType">Type of a property.</param>
        /// <returns><c>true</c> if [is entity type] [the specified a property type]; otherwise, <c>false</c>.</returns>
        private static bool IsEntityType([NotNull] Type aPropertyType)
        {
            var lIsEntityType = aPropertyType.GetInterfaces().Any(aType => aType.IsGenericType && aType.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));

            return lIsEntityType;
        }

        /// <summary>
        /// Determines whether [is simple generic bindable entity collection] [the specified a property type].
        /// </summary>
        /// <param name="aPropertyType">Type of a property.</param>
        /// <returns><c>true</c> if [is simple generic bindable entity collection] [the specified a property type]; otherwise, <c>false</c>.</returns>
        private static bool IsSimpleGenericBindableEntityCollection([NotNull] Type aPropertyType)
        {
            var lIsSimpleGenericBindableCollection = aPropertyType.IsGenericType &&
                                                    (aPropertyType.GetGenericTypeDefinition() == typeof(IList<>) || aPropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                                                     aPropertyType.GetGenericTypeDefinition() == typeof(ISet<>) || aPropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            var lIsSimpleGenericBindableEntityCollection = lIsSimpleGenericBindableCollection && IsEntityType(aPropertyType.GetGenericArguments().First());

            return lIsSimpleGenericBindableEntityCollection;
        }

        /// <summary>
        /// Called when the model is updating. We handle updating the Id property here because it gets filtered out
        /// of the normal MVC2 property binding.
        /// </summary>
        /// <param name="aControllerContext">The context within which the controller operates. The context information includes the controller, HTTP content, request context, and route data.</param>
        /// <param name="aBindingContext">The context within which the model is bound. The context includes information such as the model object, model name, model type, property filter, and value provider.</param>
        /// <returns>true if the model is updating; otherwise, false.</returns>
        protected override bool OnModelUpdating([NotNull] ControllerContext aControllerContext, [NotNull] ModelBindingContext aBindingContext)
        {
            if (IsEntityType(aBindingContext.ModelType))
            {
                // handle the Id property
                var lIDProperty = (from PropertyDescriptor lProperty in TypeDescriptor.GetProperties(aBindingContext.ModelType) where lProperty.Name == lcIdPropertyName select lProperty).SingleOrDefault();

                BindProperty(aControllerContext, aBindingContext, lIDProperty);
            }

            return base.OnModelUpdating(aControllerContext, aBindingContext);
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="aControllerContext">A controller context.</param>
        /// <param name="aBindingContext">A binding context.</param>
        /// <param name="aPropertyDescriptor">A property descriptor.</param>
        /// <param name="aValue">A value.</param>
        protected override void SetProperty([NotNull] ControllerContext aControllerContext, [NotNull] ModelBindingContext aBindingContext, [NotNull] PropertyDescriptor aPropertyDescriptor, [NotNull] object aValue)
        {
            if (aPropertyDescriptor.Name == lcIdPropertyName)
            {
                SetIdProperty(aBindingContext, aPropertyDescriptor, aValue);
            }
            else if (aValue as IEnumerable != null && IsSimpleGenericBindableEntityCollection(aPropertyDescriptor.PropertyType))
            {
                SetEntityCollectionProperty(aBindingContext, aPropertyDescriptor, aValue);
            }
            else
            {
                base.SetProperty(aControllerContext, aBindingContext, aPropertyDescriptor, aValue);
            }
        }

        /// <summary>
        /// If the property being bound is an Id property, then use reflection to get past the
        /// protected visibility of the Id property, accordingly.
        /// </summary>
        /// <param name="aBindingContext">A binding context.</param>
        /// <param name="aPropertyDescriptor">A property descriptor.</param>
        /// <param name="aValue">A value.</param>
        private static void SetIdProperty([NotNull] ModelBindingContext aBindingContext, [NotNull] PropertyDescriptor aPropertyDescriptor, [CanBeNull] object aValue)
        {
            var lIDType = aPropertyDescriptor.PropertyType;
            object lTypedId;

            if (aValue == null)
            {
                lTypedId = lIDType.IsValueType ? Activator.CreateInstance(lIDType) : null;
            }
            else
            {
                lTypedId = Convert.ChangeType(aValue, lIDType);
            }

            // First, look to see if there's an Id property declared on the entity itself;
            // e.g., using the new keyword
            var lIDProperty = aBindingContext.ModelType.GetProperty(aPropertyDescriptor.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) ??
                             aBindingContext.ModelType.GetProperty(aPropertyDescriptor.Name, BindingFlags.Public | BindingFlags.Instance);

            // If an Id property wasn't found on the entity, then grab the Id property from
            // the entity base class

            // Set the value of the protected Id property
            lIDProperty.SetValue(aBindingContext.Model, lTypedId, null);
        }

        /// <summary>
        /// If the property being bound is a simple, generic collection of entiy objects, then use
        /// reflection to get past the protected visibility of the collection property, if necessary.
        /// </summary>
        /// <param name="aBindingContext">A binding context.</param>
        /// <param name="aPropertyDescriptor">A property descriptor.</param>
        /// <param name="aValue">A value.</param>
        private static void SetEntityCollectionProperty([NotNull] ModelBindingContext aBindingContext, [NotNull] PropertyDescriptor aPropertyDescriptor, [NotNull] object aValue)
        {
            var lEntityCollection = aPropertyDescriptor.GetValue(aBindingContext.Model);
            if (lEntityCollection != aValue)
            {
                var lEntityCollectionType = lEntityCollection.GetType();

                foreach (var lEntity in (IEnumerable)aValue)
                {
                    lEntityCollectionType.InvokeMember("Add", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, lEntityCollection, new[] { lEntity });
                }
            }
        }
    }
}