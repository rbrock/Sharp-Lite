using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using SharpLite.Domain.DataInterfaces;

// This is needed for the DependencyResolver...wish they would've just used Common Service Locator!

namespace SharpLite.Domain.Validators{
    /// <summary>
    /// Due to the fact that .NET does not support generic attributes, this only works for entity
    /// types having an Id of type int.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    sealed public class HasUniqueDomainSignatureAttribute : ValidationAttribute{
        /// <summary>
        /// Determines whether the specified a value is valid.
        /// </summary>
        /// <param name="aValue">A value.</param>
        /// <param name="aValidationContext">A validation context.</param>
        /// <exception cref="System.InvalidOperationException">This validator must be used at the class level of an <see><cref>IEntityWithTypedId{int}</cref></see></exception>
        /// <exception cref="System.TypeLoadException"><see cref="IEntityDuplicateChecker" /> has not been registered with IoC</exception>
        [CanBeNull]
        protected override ValidationResult IsValid([CanBeNull] object aValue, [NotNull] ValidationContext aValidationContext){
            if (aValue == null) return null;

            var lEntityToValidate = aValue as IEntityWithTypedId<int>;

            if (lEntityToValidate == null) throw new InvalidOperationException($"This validator must be used at the class level of an IEntityWithTypedId<int>. The type you provided was {aValue.GetType()}");

            IEntityDuplicateChecker lDuplicateChecker = DependencyResolver.Current.GetService<IEntityDuplicateChecker>();

            if (lDuplicateChecker == null) throw new TypeLoadException("IEntityDuplicateChecker has not been registered with IoC");

            return lDuplicateChecker.DoesDuplicateExistWithTypedIdOf(lEntityToValidate)
                       ? new ValidationResult(String.Empty)
                       : null;
        }
    }

    /// <summary>Defines the methods that simplify service location and dependency resolution.</summary>
    public interface IDependencyResolver
    {
        /// <summary>Resolves singly registered services that support arbitrary object creation.</summary>
        /// <returns>The requested service or object.</returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        object GetService(Type serviceType);

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T.</returns>
        T GetService<T>();

        /// <summary>Resolves multiply registered services.</summary>
        /// <returns>The requested services.</returns>
        /// <param name="serviceType">The type of the requested services.</param>
        IEnumerable<object> GetServices(Type serviceType);
    }

    /// <summary>
    /// Class DependencyResolver.
    /// </summary>
    public class DependencyResolver
    {
        private static DependencyResolver _instance = new DependencyResolver();
        private IDependencyResolver _current;
        private DependencyResolver.CacheDependencyResolver _currentCache;

        /// <summary>Gets the implementation of the dependency resolver.</summary>
        /// <returns>The implementation of the dependency resolver.</returns>
        public static IDependencyResolver Current { get { return DependencyResolver._instance.InnerCurrent; } }

        internal static IDependencyResolver CurrentCache { get { return DependencyResolver._instance.InnerCurrentCache; } }

        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
        /// <returns>The implementation of the dependency resolver.</returns>
        public IDependencyResolver InnerCurrent { get { return this._current; } }

        internal IDependencyResolver InnerCurrentCache { get { return (IDependencyResolver)this._currentCache; } }

        /// <summary>Initializes a new instance of the <see cref="T:DependencyResolver" /> class.</summary>
        public DependencyResolver()
        {
            this.InnerSetResolver((IDependencyResolver)new DependencyResolver.DefaultDependencyResolver());
        }

        /// <summary>Provides a registration point for dependency resolvers, using the specified dependency resolver interface.</summary>
        /// <param name="resolver">The dependency resolver.</param>
        public static void SetResolver(IDependencyResolver resolver)
        {
            DependencyResolver._instance.InnerSetResolver(resolver);
        }

        /// <summary>Provides a registration point for dependency resolvers using the provided common service locator when using a service locator interface.</summary>
        /// <param name="commonServiceLocator">The common service locator.</param>
        public static void SetResolver(object commonServiceLocator)
        {
            DependencyResolver._instance.InnerSetResolver(commonServiceLocator);
        }

        /// <summary>Provides a registration point for dependency resolvers using the specified service delegate and specified service collection delegates.</summary>
        /// <param name="getService">The service delegate.</param>
        /// <param name="getServices">The services delegates.</param>
        public static void SetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
        {
            DependencyResolver._instance.InnerSetResolver(getService, getServices);
        }

        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
        /// <param name="resolver">The object that implements the dependency resolver.</param>
        public void InnerSetResolver(IDependencyResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException("resolver");
            this._current = resolver;
            this._currentCache = new DependencyResolver.CacheDependencyResolver(this._current);
        }

        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
        /// <param name="commonServiceLocator">The common service locator.</param>
        public void InnerSetResolver(object commonServiceLocator)
        {
            if (commonServiceLocator == null) throw new ArgumentNullException("commonServiceLocator");
            Type type = commonServiceLocator.GetType();
            MethodInfo method1 = type.GetMethod("GetInstance", new Type[1]{
                                                                   typeof(Type)
                                                               });
            MethodInfo method2 = type.GetMethod("GetAllInstances", new Type[1]{
                                                                       typeof(Type)
                                                                   });
            if (method1 == (MethodInfo)null || method1.ReturnType != typeof(object) || (method2 == (MethodInfo)null || method2.ReturnType != typeof(IEnumerable<object>)))
                throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "SistemaInfo: MvcResources.DependencyResolver_DoesNotImplementICommonServiceLocator: {0}", new object[1]{
                                                                                                                                                                                   (object) type.FullName
                                                                                                                                                                               }), "commonServiceLocator");
            this.InnerSetResolver((IDependencyResolver)new DependencyResolver.DelegateBasedDependencyResolver((Func<Type, object>)Delegate.CreateDelegate(typeof(Func<Type, object>), commonServiceLocator, method1), (Func<Type, IEnumerable<object>>)Delegate.CreateDelegate(typeof(Func<Type, IEnumerable<object>>), commonServiceLocator, method2)));
        }

        /// <summary>This API supports the ASP.NET MVC infrastructure and is not intended to be used directly from your code.</summary>
        /// <param name="getService">The function that provides the service.</param>
        /// <param name="getServices">The function that provides the services.</param>
        public void InnerSetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
        {
            if (getService == null) throw new ArgumentNullException("getService");
            if (getServices == null) throw new ArgumentNullException("getServices");
            this.InnerSetResolver((IDependencyResolver)new DependencyResolver.DelegateBasedDependencyResolver(getService, getServices));
        }

        private sealed class CacheDependencyResolver : IDependencyResolver
        {
            private readonly ConcurrentDictionary<Type, object> _cache = new ConcurrentDictionary<Type, object>();
            private readonly ConcurrentDictionary<Type, IEnumerable<object>> _cacheMultiple = new ConcurrentDictionary<Type, IEnumerable<object>>();
            private readonly IDependencyResolver _resolver;

            public CacheDependencyResolver(IDependencyResolver resolver)
            {
                this._resolver = resolver;
            }

            public object GetService(Type serviceType)
            {
                return this._cache.GetOrAdd(serviceType, new Func<Type, object>(this._resolver.GetService));
            }

            #region Implementation of IDependencyResolver

            public T GetService<T>(){
                return (T) this.GetService(typeof(T));
            }

            #endregion

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return this._cacheMultiple.GetOrAdd(serviceType, new Func<Type, IEnumerable<object>>(this._resolver.GetServices));
            }
        }

        private class DefaultDependencyResolver : IDependencyResolver
        {
            public object GetService(Type serviceType)
            {
                if (!serviceType.IsInterface)
                {
                    if (!serviceType.IsAbstract)
                    {
                        try
                        {
                            return Activator.CreateInstance(serviceType);
                        }
                        catch
                        {
                            return (object)null;
                        }
                    }
                }
                return (object)null;
            }

            public T GetService<T>()
            {
                return (T)this.GetService(typeof(T));
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return Enumerable.Empty<object>();
            }
        }

        private class DelegateBasedDependencyResolver : IDependencyResolver
        {
            private Func<Type, object> _getService;
            private Func<Type, IEnumerable<object>> _getServices;

            public DelegateBasedDependencyResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices)
            {
                this._getService = getService;
                this._getServices = getServices;
            }

            public object GetService(Type type)
            {
                try
                {
                    return this._getService(type);
                }
                catch
                {
                    return (object)null;
                }
            }

            public T GetService<T>()
            {
                return (T)this.GetService(typeof(T));
            }

            public IEnumerable<object> GetServices(Type type)
            {
                return this._getServices(type);
            }
        }
    }
}