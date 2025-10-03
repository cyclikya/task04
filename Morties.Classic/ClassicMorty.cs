using System;
using System.Linq;
using Randm.API;

namespace Morties.Classic
{
    public class ClassicMorty : IMorty
    {
        private IGameHost _host;
        public string Name => "ClassicMorty";

        public void Initialize(IGameHost host) => _host = host;

        public int ChooseHideBox(int boxCount)
        {
            return _host.RequestFairRandom(boxCount, "choose hiding box");
        }

        public int[] ChooseBoxesToKeep(int boxCount, int rickInitialChoice, int hiddenIndex)
        {
            if (rickInitialChoice != hiddenIndex)
            {
                return new int[] { rickInitialChoice, hiddenIndex };
            }
            else
            {
                int pick = _host.RequestFairRandom(boxCount - 1, "choose one other box to keep (Rick guessed correctly)");
                var others = Enumerable.Range(0, boxCount).Where(i => i != rickInitialChoice).ToArray();
                return new int[] { rickInitialChoice, others[pick] };
            }
        }

        public double TheoreticalSwitchWinProbability(int boxCount)
        {
            return (double)(boxCount - 1) / boxCount;
        }
    }
}
