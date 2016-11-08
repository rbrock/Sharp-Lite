using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;

namespace SharpLite.NHibernateProvider
{
    /// <summary>
    /// Taken from http://nhforge.org/blogs/nhibernate/archive/2011/03/03/effective-nhibernate-session-management-for-web-apps.aspx
    /// </summary>
    public class LazySessionContext : ICurrentSessionContext
    {
        /// <summary>
        /// The local constant to current session context key
        /// </summary>
        private const string lcCurrentSessionContextKey = "NHibernateCurrentSessionFactory";

        /// <summary>
        /// The internal contract between the ISessionFactory and other parts such as implementors of IType
        /// </summary>
        private readonly ISessionFactoryImplementor mSessionFactoryImplementor;

        /// <summary>
        /// Gets or sets the factory map in context.
        /// </summary>
        /// <value>The factory map in context.</value>
        [CanBeNull]
        private static IDictionary<ISessionFactory, Lazy<ISession>> FactoryMapInContext {
            get
            {
                return (IsInWebContext()
                    ? HttpContext.Current.Items[lcCurrentSessionContextKey]
                    : CallContext.GetData(lcCurrentSessionContextKey)) as IDictionary<ISessionFactory, Lazy<ISession>>;
            }
            set
            {
                if (IsInWebContext())
                {
                    HttpContext.Current.Items[lcCurrentSessionContextKey] = value;
                }
                else
                {
                    CallContext.SetData(lcCurrentSessionContextKey, value);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazySessionContext" /> class.
        /// </summary>
        /// <param name="aSessionFactoryImplementor">The session factory implementor.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aSessionFactoryImplementor parameter is null.</exception>
        public LazySessionContext([NotNull] ISessionFactoryImplementor aSessionFactoryImplementor)
        {
            if (aSessionFactoryImplementor == null) throw new ArgumentNullException(nameof(aSessionFactoryImplementor));

            mSessionFactoryImplementor = aSessionFactoryImplementor;
        }

        /// <summary>
        /// Determines whether [is in web context].
        /// </summary>
        /// <returns><c>true</c> if [is in web context]; otherwise, <c>false</c>.</returns>
        private static bool IsInWebContext() {
            return HttpContext.Current != null;
        }

        /// <summary>
        /// Retrieve the current session for the session factory.
        /// </summary>
        /// <returns>The current session.</returns>
        [CanBeNull]
        public ISession CurrentSession()
        {
            Lazy<ISession> lInitializer;

            var lCurrentSessionFactoryMap = GetCurrentFactoryMap();

            return !lCurrentSessionFactoryMap.TryGetValue(mSessionFactoryImplementor, out lInitializer) ? null : lInitializer.Value;
        }

        /// <summary>
        /// Provides the CurrentMap of SessionFactories.
        /// If there is no map create/store and return a new one.
        /// </summary>
        /// <returns>The dictionary collection of session factory and lazy session.</returns>
        [NotNull]
        private static IDictionary<ISessionFactory, Lazy<ISession>> GetCurrentFactoryMap()
        {
            var lCurrentFactoryMap = FactoryMapInContext;

            if (lCurrentFactoryMap == null)
            {
                lCurrentFactoryMap = new Dictionary<ISessionFactory, Lazy<ISession>>();
                FactoryMapInContext = lCurrentFactoryMap;
            }

            return lCurrentFactoryMap;
        }

        /// <summary>
        /// Unbind the current session of the session factory.
        /// </summary>
        /// <param name="aSessionFactory">The session factory.</param>
        /// <returns>ISession.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aSessionFactory parameter is null.</exception>
        [CanBeNull]
        public static ISession UnBind([NotNull] ISessionFactory aSessionFactory)
        {
            if (aSessionFactory == null) throw new ArgumentNullException(nameof(aSessionFactory));

            var lMap = GetCurrentFactoryMap();
            var lSessionInitializer = lMap[aSessionFactory];
            lMap[aSessionFactory] = null;
            if (lSessionInitializer == null || !lSessionInitializer.IsValueCreated) return null;
            return lSessionInitializer.Value;
        }

        /// <summary>
        /// Bind a new sessionInitializer to the context of the sessionFactory.
        /// </summary>
        /// <param name="aSessionInitializer">A session initializer.</param>
        /// <param name="aSessionFactory">A session factory.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aSessionInitializer parameter or aSessionFactory parameter is null.</exception>
        public static void Bind([NotNull] Lazy<ISession> aSessionInitializer, [NotNull] ISessionFactory aSessionFactory)
        {
            if (aSessionInitializer == null) throw new ArgumentNullException(nameof(aSessionInitializer));
            if (aSessionFactory == null) throw new ArgumentNullException(nameof(aSessionFactory));

            var lMap = GetCurrentFactoryMap();
            lMap[aSessionFactory] = aSessionInitializer;
        }
    }
}
