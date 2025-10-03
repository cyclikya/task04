using System;
using Randm.API;

namespace Randm
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var parsed = ArgParser.Parse(args);

                var loader = new MortyLoader();
                var morty = loader.LoadMorty(parsed.MortyAssemblyPath, parsed.MortyTypeName);

                var game = new GameCore(parsed.BoxCount, morty);
                game.Run();
                return 0;
            }
            catch (ArgParseException ex)
            {
                Console.Error.WriteLine("Argument error: " + ex.Message);
                Console.Error.WriteLine();
                Console.Error.WriteLine("Usage:");
                Console.Error.WriteLine("  Randm <boxCount> <path-to-morty-assembly.dll> [MortyFullTypeName]");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Example:");
                Console.Error.WriteLine(@"  Randm 3 C:\work\Morties.Classic.dll Morties.Classic.ClassicMorty");
                return 1;
            }
            catch (MortyLoadException ex)
            {
                Console.Error.WriteLine("Morty load error: " + ex.Message);
                return 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unexpected error: " + ex.Message);
                return 3;
            }
        }
    }
}
