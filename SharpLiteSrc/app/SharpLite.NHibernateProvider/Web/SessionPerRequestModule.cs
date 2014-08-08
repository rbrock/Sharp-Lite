using System;
using System.Web;
using NHibernate;
using System.Collections.Generic;
// This is needed for the DependencyResolver...wish they would've just used Common Service Locator!
using System.Web.Mvc;
using System.Linq;
using SharpLite.NHibernateProvider.Annotations;

namespace SharpLite.NHibernateProvider.Web
{
    /// <summary>
    /// Taken from http://nhforge.org/blogs/nhibernate/archive/2011/03/03/effective-nhibernate-session-management-for-web-apps.aspx
    /// </summary>
    public class SessionPerRequestModule : IHttpModule
    {
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="aContext">The context.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aContext parameter is null.</exception>
        public void Init([NotNull] HttpApplication aContext) {
            if (aContext == null) throw new ArgumentNullException("aContext");

            aContext.BeginRequest += ContextBeginRequest;
            aContext.EndRequest += ContextEndRequest;
            aContext.Error += ContextError;
        }

        /// <summary>
        /// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
        /// The BeginRequest event signals the creation of any given new request. This event is always raised and
        /// is always the first event to occur during the processing of a request.
        /// </summary>
        /// <param name="aSender">The object sender.</param>
        /// <param name="aEventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void ContextBeginRequest([NotNull] object aSender, [NotNull] EventArgs aEventArgs)
        {
            foreach (var lSessionFactory in GetSessionFactoryList())
            {
                var lLocalFactory = lSessionFactory;

                LazySessionContext.Bind(new Lazy<ISession>(() => BeginSession(lLocalFactory)), lSessionFactory);
            }
        }

        /// <summary>
        /// Retrieves all ISessionFactory instances via IoC
        /// </summary>
        /// <returns>The collection of session factory.</returns>
        /// <exception cref="System.TypeLoadException">At least one ISessionFactory has not been registered with IoC</exception>
        [NotNull]
        private static IEnumerable<ISessionFactory> GetSessionFactoryList()
        {
            var lSessionFactories = DependencyResolver.Current.GetServices<ISessionFactory>().ToList();

            if (lSessionFactories == null || !lSessionFactories.Any()) throw new TypeLoadException("At least one ISessionFactory has not been registered with IoC");

            return lSessionFactories;
        }

        /// <summary>
        /// Create a database connection, open a ISession on it and begin a unit of work.
        /// </summary>
        /// <param name="aSessionFactory">The session factory.</param>
        /// <returns>The session.</returns>
        [NotNull]
        private static ISession BeginSession([NotNull] ISessionFactory aSessionFactory)
        {
            var lSession = aSessionFactory.OpenSession();

            lSession.BeginTransaction();

            return lSession;
        }

        /// <summary>
        /// Occurs as the last event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
        /// The EndRequest event is always raised when the CompleteRequest method is called.
        /// </summary>
        /// <param name="aSender">The object sender.</param>
        /// <param name="aEventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void ContextEndRequest([NotNull] object aSender, [NotNull] EventArgs aEventArgs)
        {
            foreach (var lSessionfactory in GetSessionFactoryList())
            {
                var lSession = LazySessionContext.UnBind(lSessionfactory);
                if (lSession == null) continue;
                EndSession(lSession);
            }
        }

        /// <summary>
        /// Verify if the transaction is in progress and, if commit, flush the associated ISession and end the 
        /// unit of work, or force the underlying transaction to roll back and then end the ISession by 
        /// disconnecting from the ADO.NET connection and cleaning up.
        /// </summary>
        /// <param name="aSession">The session.</param>
        /// <param name="aCommitTransaction">if set to <c>true</c> [commit transaction].</param>
        private static void EndSession([NotNull] ISession aSession, bool aCommitTransaction = true)
        {
            try
            {
                if (aSession.Transaction != null && aSession.Transaction.IsActive)
                {
                    if (aCommitTransaction)
                    {
                        try
                        {
                            aSession.Transaction.Commit();
                        }
                        catch
                        {
                            aSession.Transaction.Rollback();
                            throw;
                        }
                    }
                    else
                    {
                        aSession.Transaction.Rollback();
                    }
                }
            }
            finally
            {
                if (aSession.IsOpen)
                    aSession.Close();

                aSession.Dispose();
            }
        }

        /// <summary>
        /// Occurs when an unhandled exception is thrown.
        /// The exception that raises the Error event can be accessed by a call to the GetLastError method.
        /// If your application generates custom error output, suppress the default error message that is
        /// generated by ASP.NET by a call to the ClearError method.
        /// </summary>
        /// <param name="aSender">The object sender.</param>
        /// <param name="aEventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void ContextError([NotNull] object aSender, [NotNull] EventArgs aEventArgs)
        {
            foreach (var lSession in GetSessionFactoryList().Select(LazySessionContext.UnBind).Where(aSession => aSession != null))
            {
                EndSession(lSession, false);
            }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose() { }
    }
}
