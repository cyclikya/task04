using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Randm.API;

namespace Randm
{
    public class MortyLoadException : Exception { public MortyLoadException(string m) : base(m) { } }

    public class MortyLoader
    {
        public IMorty LoadMorty(string assemblyPath, string typeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(assemblyPath))
                    throw new MortyLoadException("Assembly path is empty.");

                var full = Path.GetFullPath(assemblyPath);

                if (!File.Exists(full))
                    throw new MortyLoadException($"Assembly not found: {full}");

                var asm = Assembly.LoadFrom(full);

                Type chosen = null;

                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    chosen = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
                    if (chosen == null)
                        throw new MortyLoadException($"Type '{typeName}' not found in assembly '{full}'.");
                }
                else
                {
                    var types = asm.GetTypes()
                                   .Where(t => typeof(IMorty).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                                   .ToArray();

                    if (types.Length == 0)
                        throw new MortyLoadException("No types implementing Randm.API.IMorty were found in the assembly.");

                    if (types.Length > 1)
                    {
                        chosen = types[0];
                        Console.WriteLine("Warning: multiple types implement IMorty; using first found: " + chosen.FullName);
                    }
                    else
                        chosen = types[0];
                }

                var inst = Activator.CreateInstance(chosen);
                if (inst is IMorty morty)
                    return morty;

                throw new MortyLoadException($"Type {chosen.FullName} does not implement IMorty.");
            }
            catch (ReflectionTypeLoadException rtle)
            {
                var msgs = string.Join("; ", rtle.LoaderExceptions.Select(e => e.Message));
                throw new MortyLoadException("Failed to load types from assembly: " + msgs);
            }
            catch (Exception ex) when (!(ex is MortyLoadException))
            {
                throw new MortyLoadException("Failed to load Morty: " + ex.Message);
            }
        }
    }
}
