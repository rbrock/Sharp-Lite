using System.Collections.Generic;
using NHibernate.Cfg;
using SharpLite.NHibernateProvider.Annotations;

namespace SharpLite.NHibernateProvider.ConfigurationCaching
{
    /// <summary>
    ///     Interface for providing caching capability for an <see cref = "Configuration" /> object.
    /// </summary>
    public interface INHibernateConfigurationCache
    {
        /// <summary>
        /// Load the <see cref="Configuration" /> object from a cache.
        /// </summary>
        /// <param name="aConfigKey">Key value to provide a unique name to the cached <see cref="Configuration" />.</param>
        /// <param name="aConfigPath">NHibernate configuration xml file.  This is used to determine if the
        /// cached <see cref="Configuration" /> is out of date or not.</param>
        /// <param name="aMappingAssemblies">List of assemblies containing HBM files.</param>
        /// <returns>If an up to date cached object is available, a <see cref="Configuration" /> object, otherwise null.</returns>
        [CanBeNull]
        Configuration LoadConfiguration([NotNull] string aConfigKey, [NotNull] string aConfigPath, [NotNull] IEnumerable<string> aMappingAssemblies);

        /// <summary>
        /// Save the <see cref="Configuration" /> object to a cache.
        /// </summary>
        /// <param name="aConfigKey">Key value to provide a unique name to the cached <see cref="Configuration" />.</param>
        /// <param name="aConfiguration">Configuration object to save.</param>
        void SaveConfiguration([NotNull] string aConfigKey, [NotNull] Configuration aConfiguration);
    }
}
