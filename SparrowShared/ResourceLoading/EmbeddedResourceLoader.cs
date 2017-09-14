using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sparrow.ResourceLoading
{
    /// <summary>
    /// Utility class that can be used to find and load embedded resources into memory.
    /// </summary>
    public class EmbeddedResourceLoader
    {

        private readonly string assemblyName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_assemblyName">The assemby name this loader will load from.</param>
        public EmbeddedResourceLoader(string _assemblyName)
        {
            assemblyName = _assemblyName + ".";
        }
        ///<summary>
        /// Attempts to find and return the given resource from within the calling assembly.
        /// </summary>
        /// <returns>The embedded resource as a stream.</returns>
        /// <param name="resourceFileName">Resource file name.</param>
        public Stream GetEmbeddedResourceStream(string resourceFileName)
        {
            return GetEmbeddedResourceStream(Assembly.GetCallingAssembly(), assemblyName + resourceFileName);
        }
        
        /// <summary>
        /// Attempts to find and return the given resource from within the calling assembly.
        /// </summary>
        /// <returns>The embedded resource as a byte array.</returns>
        /// <param name="resourceFileName">Resource file name.</param>
        public byte[] GetEmbeddedResourceBytes(string resourceFileName)
        {
            return GetEmbeddedResourceBytes(Assembly.GetCallingAssembly(), assemblyName + resourceFileName);
        }
        
        /// <summary>
        /// Attempts to find and return the given resource from within the calling assembly.
        /// </summary>
        /// <returns>The embedded resource as a string.</returns>
        /// <param name="resourceFileName">Resource file name.</param>
        public string GetEmbeddedResourceString(string resourceFileName)
        {
            return GetEmbeddedResourceString(Assembly.GetCallingAssembly(), assemblyName + resourceFileName);
        }

        /// <summary>
        /// Attempts to find and return the given resource from within the specified assembly.
        /// </summary>
        /// <returns>The embedded resource stream.</returns>
        /// <param name="assembly">Assembly.</param>
        /// <param name="resourceFileName">Resource file name.</param>
        public Stream GetEmbeddedResourceStream(Assembly assembly, string resourceFileName)
        {
            var resourceNames = assembly.GetManifestResourceNames();

            var resourcePaths = resourceNames
                .Where(x => x.EndsWith(resourceFileName, StringComparison.CurrentCultureIgnoreCase))
                .ToArray();

            if (!resourcePaths.Any())
            {
                throw new Exception($"Resource ending with {resourceFileName} not found.");
            }
            if (resourcePaths.Count() > 1)
            {
                throw new Exception(
                    $"Multiple resources ending with {resourceFileName} found: {Environment.NewLine}{string.Join(Environment.NewLine, resourcePaths)}");
            }

            return assembly.GetManifestResourceStream(resourcePaths.Single());
        }

        /// <summary>
        /// Attempts to find and return the given resource from within the specified assembly.
        /// </summary>
        /// <returns>The embedded resource as a byte array.</returns>
        /// <param name="assembly">Assembly.</param>
        /// <param name="resourceFileName">Resource file name.</param>
        public byte[] GetEmbeddedResourceBytes(Assembly assembly, string resourceFileName)
        {
            var stream = GetEmbeddedResourceStream(assembly, resourceFileName);

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Attempts to find and return the given resource from within the specified assembly.
        /// </summary>
        /// <returns>The embedded resource as a string.</returns>
        /// <param name="assembly">Assembly.</param>
        /// <param name="resourceFileName">Resource file name.</param>
        public string GetEmbeddedResourceString(Assembly assembly, string resourceFileName)
        {
            var stream = GetEmbeddedResourceStream(assembly, resourceFileName);

            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public void _DebugPrintAllResources()
        {
            foreach (var s in typeof(EmbeddedResourceLoader).Assembly.GetManifestResourceNames())
            {
                System.Diagnostics.Debug.WriteLine(s);
            }
            System.Diagnostics.Debug.WriteLine("done.");
        }
    }
}
