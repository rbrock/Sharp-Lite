using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SharpLite.NHibernateProvider.Annotations;

namespace SharpLite.NHibernateProvider.ConfigurationCaching
{
    /// <summary>
    /// Class FileCache.
    /// </summary>
    public static class FileCache
    {
        /// <summary>
        /// Deserializes a data file into an object of type {T}.
        /// </summary>
        /// <typeparam name="T">Type of object to deseralize and return.</typeparam>
        /// <param name="aPath">Full aPath to file containing seralized data.</param>
        /// <returns>If object is successfully deseralized, the object of type {T},
        /// otherwise null.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the aPath parameter is null or empty or white space.</exception>
        [CanBeNull]
        public static T RetrieveFromCache<T>([NotNull] string aPath) where T : class
        {
            if (string.IsNullOrWhiteSpace(aPath)) throw new ArgumentNullException("aPath");

            try
            {
                using (var lFile = File.Open(aPath, FileMode.Open))
                {
                    var lBinaryFormatter = new BinaryFormatter();
                    return lBinaryFormatter.Deserialize(lFile) as T;
                }
            }
            catch
            {
                // Return null if the object can't be deseralized
                return null;
            }
        }

        /// <summary>
        /// Serialize the given object of type {T} to a file at the given aPath.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="aGraph">Object to serialize and store in a file.</param>
        /// <param name="aPath">Full aPath of file to store the serialized data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the aGraph parameters is null or the aPath parameter is null or empty or white space.</exception>
        public static void StoreInCache<T>([NotNull] T aGraph, [NotNull] string aPath) where T : class
        {
            if (aGraph == null) throw new ArgumentNullException("aGraph");
            if (string.IsNullOrWhiteSpace(aPath)) throw new ArgumentNullException("aPath");

            using (var lSerializationStream = File.Open(aPath, FileMode.Create))
            {
                new BinaryFormatter().Serialize(lSerializationStream, aGraph);
            }
        }
    }
}