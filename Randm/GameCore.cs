using System;
using Randm.API;
using Spectre.Console;
using System.Linq;

namespace Randm
{

    public class GameCore : IGameHost
    {
        private readonly int _boxes;
        private readonly IMorty _morty;
        private readonly FairRandomGenerator _frg;
        private readonly Statistics _stats;

        public GameCore(int boxes, IMorty morty)
        {
            _boxes = boxes;
            _morty = morty;
            _frg = new FairRandomGenerator(this);
            _stats = new Statistics();
            _morty.Initialize(this);
        }

        public void Run()
        {
            Console.WriteLine($"Morty plugin: {_morty.Name}");
            Console.WriteLine($"Game with {_boxes} boxes.");
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== New round ===");
                int hidden = _morty.ChooseHideBox(_boxes); 
                int rickInitial = ReadInt($"Rick: choose a box [0, {_boxes}) as your initial guess:", 0, _boxes - 1);

                var kept = _morty.ChooseBoxesToKeep(_boxes, rickInitial, hidden);
                if (kept == null || kept.Length != 2)
                {
                    Console.WriteLine("Morty plugin returned invalid kept boxes; must return exactly 2 distinct indexes.");
                    return;
                }

                Console.WriteLine($"Morty: I'm keeping the boxes {kept[0]} and {kept[1]}.");
                Console.WriteLine("Morty: You can switch (enter 0), or stick with your original choice (enter 1).");
                int action = ReadInt("Enter 0=switch, 1=stay:", 0, 1);

                bool switched = action == 0;
                int finalChoice = switched ? kept.First(x => x != rickInitial) : rickInitial;

                Console.WriteLine($"Morty: Let's open your box {finalChoice}...");
                bool rickWins = finalChoice == hidden;
                if (rickWins)
                {
                    Console.WriteLine("Morty: Aww man, you won, Rick!");
                }
                else
                {
                    Console.WriteLine("Morty: Aww man, you lost, Rick. Now we gotta go on one of *my* adventures!");
                }

                _stats.RegisterRound(switched, rickWins, _morty.TheoreticalSwitchWinProbability(_boxes));

                Console.WriteLine();
                Console.WriteLine("=== Reveals for this round (verifiability) ===");
                foreach (var rec in _frg.ConsumeAndGetRecentRecords())
                {
                    Console.WriteLine($"Morty: HMAC={rec.HmacHex}");
                    Console.WriteLine($"Morty: My secret was {rec.MortyValue}. KEY={rec.KeyHex}");
                    Console.WriteLine($"Morty: (Morty + Rick) % {rec.Modulo} = {rec.Final} (Rick contributed {rec.RickValue}).");
                    Console.WriteLine();
                }

                Console.Write("Play another round? (y/n) ");
                var k = Console.ReadKey(intercept: true);
                Console.WriteLine();
                if (k.KeyChar == 'n' || k.KeyChar == 'N')
                {
                    PrintSummary();
                    break;
                }
            }
        }

        private void PrintSummary()
        {
            Console.WriteLine();
            Console.WriteLine("                 GAME STATS ");
            var table = new Table();
            table.AddColumn("Game results");
            table.AddColumn("Rick switched");
            table.AddColumn("Rick stayed");

            table.AddRow("Rounds", _stats.RoundsSwitched.ToString(), _stats.RoundsStayed.ToString());
            table.AddRow("Wins", _stats.WinsWhenSwitched.ToString(), _stats.WinsWhenStayed.ToString());
            table.AddRow("P (estimate)", _stats.EstimatedSwitchWinProbability().ToString("0.000"), _stats.EstimatedStayWinProbability().ToString("0.000"));
            table.AddRow("P (exact)", _stats.TheoreticalSwitchProbability.ToString("0.000"), (1.0 - _stats.TheoreticalSwitchProbability).ToString("0.000"));

            AnsiConsole.Render(table);
        }

        private int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                Console.Write($"> ");
                var s = Console.ReadLine();
                if (!int.TryParse(s, out int v))
                {
                    Console.WriteLine("Please enter an integer.");
                    continue;
                }
                if (v < min || v > max)
                {
                    Console.WriteLine($"Value must be between {min} and {max} (inclusive).");
                    continue;
                }
                return v;
            }
        }

        public int RequestFairRandom(int n, string purpose)
        {
            return _frg.Generate(n, purpose);
        }

        public void Log(string message)
        {
            Console.WriteLine($"Morty: {message}");
        }
    }
}
