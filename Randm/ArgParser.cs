using System;
using System.IO;

namespace Randm
{
    public class ParsedArgs
    {
        public int BoxCount { get; init; }
        public string MortyAssemblyPath { get; init; } = null!;
        public string MortyTypeName { get; init; } = null!;
    }

    public class ArgParseException : Exception { public ArgParseException(string m): base(m){} }

    public static class ArgParser
    {

        public static ParsedArgs Parse(string[] args)
        {
            if (args == null || args.Length == 0)
                throw new ArgParseException("No arguments provided.");

            if (args.Length < 2)
                throw new ArgParseException("Too few arguments. Expected: <boxCount> <morty-assembly-or-name> [MortyFullTypeName].");

            if (!int.TryParse(args[0], out int boxCount))
                throw new ArgParseException("First argument must be an integer (number of boxes).");

            if (boxCount <= 2)
                throw new ArgParseException("Number of boxes must be greater than 2.");

            string assemblyArg = args[1];
            string typeName = args.Length >= 3 ? args[2] : null;

            var resolved = ResolveAssemblyPath(assemblyArg, typeName);
            if (resolved == null)
                throw new ArgParseException($"Could not find assembly for '{assemblyArg}' (looked in current dir and ./plugins).");

            return new ParsedArgs
            {
                BoxCount = boxCount,
                MortyAssemblyPath = resolved,
                MortyTypeName = typeName
            };
        }

        private static string[] SearchFolders()
        {
            var cwd = Directory.GetCurrentDirectory();
            var plugins = Path.Combine(cwd, "plugins");
            return new[] { cwd, plugins }.Where(Directory.Exists).ToArray();
        }

        private static string ResolveAssemblyPath(string assemblyArg, string typeName)
        {
            if (Path.IsPathRooted(assemblyArg) || assemblyArg.Contains(Path.DirectorySeparatorChar) || assemblyArg.Contains(Path.AltDirectorySeparatorChar))
            {
                var full = Path.GetFullPath(assemblyArg);
                if (File.Exists(full)) return full;
                if (!full.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    var withDll = full + ".dll";
                    if (File.Exists(withDll)) return Path.GetFullPath(withDll);
                }
                return null;
            }

            var cwd = Directory.GetCurrentDirectory();
            var candidate = Path.Combine(cwd, assemblyArg);
            if (File.Exists(candidate)) return Path.GetFullPath(candidate);
            var candidateDll = candidate.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? candidate : candidate + ".dll";
            if (File.Exists(candidateDll)) return Path.GetFullPath(candidateDll);

            var pluginsDir = Path.Combine(cwd, "plugins");
            if (Directory.Exists(pluginsDir))
            {
                var pluginCandidate = Path.Combine(pluginsDir, assemblyArg);
                if (File.Exists(pluginCandidate)) return Path.GetFullPath(pluginCandidate);
                var pluginCandidateDll = pluginCandidate.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ? pluginCandidate : pluginCandidate + ".dll";
                if (File.Exists(pluginCandidateDll)) return Path.GetFullPath(pluginCandidateDll);
                var nested = Path.Combine(pluginsDir, assemblyArg, assemblyArg + ".dll");
                if (File.Exists(nested)) return Path.GetFullPath(nested);
            }

            var folders = SearchFolders();
            foreach (var folder in folders)
            {
                try
                {
                    var dlls = Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);
                    var match = dlls.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p).Equals(assemblyArg, StringComparison.OrdinalIgnoreCase)
                                                         || Path.GetFileName(p).IndexOf(assemblyArg, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (match != null) return Path.GetFullPath(match);
                }
                catch { /* ignore access errors */ }
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                foreach (var folder in folders)
                {
                    try
                    {
                        var dlls = Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);
                        foreach (var dll in dlls)
                        {
                            try
                            {
                                var asm = System.Reflection.Assembly.LoadFrom(dll);
                                var types = asm.GetTypes();
                                if (types.Any(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) || t.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    return Path.GetFullPath(dll);
                                }
                            }
                            catch { /* ignore load errors for dlls that are not .NET assemblies */ }
                        }
                    }
                    catch { }
                }
            }

            return null;
        }
    }
}

