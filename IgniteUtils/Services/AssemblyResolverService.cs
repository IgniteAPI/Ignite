using InstanceUtils.Models.Server;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InstanceUtils.Services
{
    /// <summary>
    /// Provides functionality for resolving and managing assemblies within the application.
    /// </summary>
    /// <remarks>This service is intended to assist with dynamic assembly resolution, enabling scenarios such
    /// as loading assemblies at runtime or resolving dependencies. It extends the <see cref="ServiceBase"/> class to
    /// integrate with the application's service infrastructure.</remarks>
    public class AssemblyResolverService : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private List<string> _searchDirs = new List<string>();


        public AssemblyResolverService(IConfigService configs)
        {

            AddRelativeSearchDir(configs.SteamCMDPath);
            AddRelativeSearchDir(configs.GamePath);

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembliesFromFolders;
        }

        /// <summary>
        /// Adds a relative search directory to the collection if it is not already present. You will need to register paths before the state is initing.
        /// </summary>
        /// <remarks>If the specified directory is already in the collection, it will not be added again. 
        /// If <paramref name="dir"/> is null, empty, or whitespace, the method performs no action.</remarks>
        /// <param name="dir">The relative directory path to add. This value cannot be null, empty, or consist only of whitespace.</param>
        public void AddRelativeSearchDir(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return;

            if (!_searchDirs.Contains(dir))
                _searchDirs.Add(dir);
        }

        private Assembly ResolveAssembliesFromFolders(object sender, ResolveEventArgs args)
        {
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";

            foreach (string dir in _searchDirs)
            {
                try
                {
                    string candidatePath = Directory.GetFiles(dir, assemblyName, SearchOption.AllDirectories).FirstOrDefault();

                    if (string.IsNullOrEmpty(candidatePath))
                        continue;

                    // Skip files that are still being downloaded (e.g. Steam downloading game files)
                    // by checking for a valid PE header before attempting to load.
                    if (!IsValidPEFile(candidatePath))
                    {
                        _logger.Debug($"Skipping incomplete/invalid PE file: {candidatePath}");
                        continue;
                    }

                    return Assembly.LoadFrom(candidatePath);
                }
                catch (BadImageFormatException)
                {
                    _logger.Debug($"Skipping assembly {assemblyName} in {dir} (file is incomplete or not a valid assembly).");
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex, $"Failed to resolve assembly {assemblyName} from directory {dir}.");
                }
            }

            //Only log if we have a valid requesting assembly
            if (args.RequestingAssembly != null)
                _logger.Warn($"Failed to resolve assembly {assemblyName} from any configured directories. Requesting Assembly: {args.RequestingAssembly}");

            return null;
        }

        /// <summary>
        /// Checks if a file has a valid PE header (MZ signature + valid PE offset).
        /// This filters out incomplete files still being downloaded.
        /// </summary>
        private static bool IsValidPEFile(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length < 64)
                    return false;

                using var reader = new BinaryReader(fs);

                // Check MZ header
                if (reader.ReadUInt16() != 0x5A4D)
                    return false;

                // Read PE header offset at 0x3C
                fs.Seek(0x3C, SeekOrigin.Begin);
                int peOffset = reader.ReadInt32();

                if (peOffset < 0 || peOffset + 4 > fs.Length)
                    return false;

                // Check PE signature
                fs.Seek(peOffset, SeekOrigin.Begin);
                return reader.ReadUInt32() == 0x00004550; // "PE\0\0"
            }
            catch
            {
                return false;
            }
        }
    }
}
