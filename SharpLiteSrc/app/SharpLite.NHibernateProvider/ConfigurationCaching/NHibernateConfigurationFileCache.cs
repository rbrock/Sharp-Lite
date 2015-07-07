using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NHibernate.Cfg;

namespace SharpLite.NHibernateProvider.ConfigurationCaching
{
    /// <summary>
    /// File cache implementation of INHibernateConfigurationCache.  Saves and loads a
    /// serialized version of <see cref="Configuration" /> to a temporary file location.
    /// </summary>
    /// <remarks>Serializing a <see cref="Configuration" /> object requires that all components
    /// that make up the Configuration object be Serializable.  This includes any custom NHibernate
    /// user types implementing <see cref="NHibernate.UserTypes.IUserType" />.</remarks>
    public class NHibernateConfigurationFileCache : INHibernateConfigurationCache
    {
        /// <summary>
        /// List of files that the cached configuration is dependent on.  If any of these
        /// files are newer than the cache file then the cache file could be out of date.
        /// </summary>
        private readonly List<string> _dependentFilePathList = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateConfigurationFileCache" /> class.
        /// </summary>
        public NHibernateConfigurationFileCache()
        {
        }

        /// <summary>
        /// Initializes new instance of the NHibernateConfigurationFileCache using the
        /// given dependentFilePathList parameter.
        /// </summary>
        /// <param name="aDependentFilePathList">List of files that the cached configuration
        /// is dependent upon.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aDependentFilePathList parameters is null.</exception>
        public NHibernateConfigurationFileCache([NotNull] IEnumerable<string> aDependentFilePathList)
        {
            if (aDependentFilePathList == null) throw new ArgumentNullException("aDependentFilePathList");

            AppendToDependentFilePaths(aDependentFilePathList);
        }

        /// <summary>
        /// Append the given list of file path to the dependent file path list.
        /// </summary>
        /// <param name="aDependentFilePathList"><see><cref>IEnumerable{string}</cref></see> list of file path.</param>
        private void AppendToDependentFilePaths([NotNull] IEnumerable<string> aDependentFilePathList)
        {
            foreach (var lDependentFilePath in aDependentFilePathList)
            {
                _dependentFilePathList.Add(FindFile(lDependentFilePath));
            }
        }

        /// <summary>
        /// Tests if the file or assembly name exists either in the application's bin folder
        /// or elsewhere.
        /// </summary>
        /// <param name="aPath">Path or file name to test for existance.</param>
        /// <returns>Full path of the file.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the file is not found.</exception>
        /// <remarks>If the path parameter does not end with ".dll" it is appended and
        /// tested if the dll file exists.</remarks>
        [NotNull]
        private static string FindFile([NotNull] string aPath)
        {
            // 1. Finds directly
            if (File.Exists(aPath)) return aPath;

            // 2. Finds in location of the executing assembly
            var lCodeBase = Assembly.GetExecutingAssembly().CodeBase;
            var lUri = new UriBuilder(lCodeBase);
            var lUriPath = Uri.UnescapeDataString(lUri.Path);
            var lCodeLocation = Path.GetDirectoryName(lUriPath);
            var lCodePath = Path.Combine(lCodeLocation, aPath);

            if (File.Exists(lCodePath)) return lCodePath;

            // 3. Add class library extension and finds directly
            const string lcClassLibraryExtension = ".dll";
            var lDllPath = (aPath.IndexOf(lcClassLibraryExtension, StringComparison.Ordinal) == -1) ? aPath.Trim() + lcClassLibraryExtension : aPath.Trim();

            if (File.Exists(lDllPath)) return lDllPath;

            // 4. Add class library extension and finds in location of the executing assembly
            var lCodeDllPath = Path.Combine(lCodeLocation, lDllPath);

            if (File.Exists(lCodeDllPath)) return lCodeDllPath;

            //
            throw new FileNotFoundException("Unable to find file.", aPath);
        }

        /// <summary>
        /// Load the <see cref="Configuration" /> object from a cache.
        /// </summary>
        /// <param name="aConfigKey">Key value to provide a unique name to the cached <see cref="Configuration" />.</param>
        /// <param name="aConfigPath">NHibernate configuration xml file.  This is used to determine if the
        /// cached <see cref="Configuration" /> is out of date or not.</param>
        /// <param name="aMappingAssemblies">String array containing assembly names where domain classes are defined.
        /// This is used by the cache to determine if the cached configuration is out of date.</param>
        /// <returns>If an up to date cached object is available, a <see cref="Configuration" />
        /// object, otherwise null.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aConfigKey or aConfigPath parameter are null or
        /// empty or white space or the aMappingAssemblies parameters is null.</exception>
        public Configuration LoadConfiguration([NotNull] string aConfigKey, [CanBeNull] string aConfigPath, [NotNull] IEnumerable<string> aMappingAssemblies)
        {
            if (aConfigKey == null) throw new ArgumentNullException("aConfigKey");
            if (aMappingAssemblies == null) throw new ArgumentNullException("aMappingAssemblies");

            var lCachedConfigPath = CachedConfigPath(aConfigKey);
            AppendToDependentFilePaths(aMappingAssemblies);

            if (!string.IsNullOrWhiteSpace(aConfigPath))
            {
                AppendToDependentFilePaths(aConfigPath);
            }

            return IsCachedConfigCurrent(lCachedConfigPath) ? FileCache.RetrieveFromCache<Configuration>(lCachedConfigPath) : null;
        }

        /// <summary>
        /// Provide a unique temporary file path/name for the cache file.
        /// </summary>
        /// <param name="aConfigKey">Key value to provide a unique name to the cached <see cref="Configuration" />.</param>
        /// <returns>Full file path.</returns>
        /// <remarks>The hash value is intended to avoid the file from conflicting
        /// with other applications also using this cache feature.</remarks>
        [NotNull]
        protected virtual string CachedConfigPath([NotNull] string aConfigKey)
        {
            var lFileName = string.Format("{0}-{1}.bin", aConfigKey, Assembly.GetCallingAssembly().CodeBase.GetHashCode());

            return Path.Combine(Path.GetTempPath(), lFileName);
        }

        /// <summary>
        /// Append the given file path to the dependentFilePathList list.
        /// </summary>
        /// <param name="aPath">File path.</param>
        private void AppendToDependentFilePaths([NotNull] string aPath)
        {
            _dependentFilePathList.Add(FindFile(aPath));
        }

        /// <summary>
        /// Tests if an existing cached configuration file is out of date or not.
        /// </summary>
        /// <param name="aCachePath">Location of the cached</param>
        /// <returns>False if the cached aConfiguration file is out of date, otherwise true.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aCachePath parameters are null.</exception>
        protected virtual bool IsCachedConfigCurrent([NotNull] string aCachePath)
        {
            if (string.IsNullOrWhiteSpace(aCachePath)) throw new ArgumentNullException("cachePath");

            return (File.Exists(aCachePath) && (File.GetLastWriteTime(aCachePath) >= GetMaxDependencyTime()));
        }

        /// <summary>
        /// Returns the latest file write time from the list of dependent file aDependentFilePathList.
        /// </summary>
        /// <returns>Latest file write time, or '1/1/1980' if list is empty.</returns>
        protected virtual DateTime GetMaxDependencyTime()
        {
            if ((_dependentFilePathList == null) || (_dependentFilePathList.Count == 0))
            {
                return DateTime.Parse("1/1/1980");
            }

            return _dependentFilePathList.Max(aPath => File.GetLastWriteTime(aPath));
        }

        /// <summary>
        /// Save the <see cref="Configuration" /> object to cache to a temporary file.
        /// </summary>
        /// <param name="aConfigKey">Key value to provide a unique name to the cached <see cref="Configuration" />.</param>
        /// <param name="aConfiguration">Configuration object to save.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aConfigKey parameter is null or empty or white space
        /// or the aConfiguration parameters is null.</exception>
        /// <exception cref="System.ArgumentNullException">
        /// aConfigKey
        /// or
        /// aConfiguration
        /// </exception>
        public void SaveConfiguration(string aConfigKey, Configuration aConfiguration)
        {
            if (string.IsNullOrWhiteSpace(aConfigKey)) throw new ArgumentNullException("aConfigKey");
            if (aConfiguration == null) throw new ArgumentNullException("aConfiguration");

            var lCachePath = CachedConfigPath(aConfigKey);
            FileCache.StoreInCache(aConfiguration, lCachePath);
            File.SetLastWriteTime(lCachePath, GetMaxDependencyTime());
        }
    }
}