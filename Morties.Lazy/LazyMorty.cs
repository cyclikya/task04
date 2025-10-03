using System;
using System.Linq;
using Randm.API;

namespace Morties.Lazy
{
    public class LazyMorty : IMorty
    {
        private IGameHost _host;
        public string Name => "LazyMorty";

        public void Initialize(IGameHost host) => _host = host;

        public int ChooseHideBox(int boxCount)
        {
            return _host.RequestFairRandom(boxCount, "lazy choose hiding box");
        }

        public int[] ChooseBoxesToKeep(int boxCount, int rickInitialChoice, int hiddenIndex)
        {
            if (rickInitialChoice == hiddenIndex)
            {
                int other = Enumerable.Range(0, boxCount).First(i => i != rickInitialChoice);
                return new int[] { rickInitialChoice, other };
            }
            else
            {
                return new int[] { rickInitialChoice, hiddenIndex };
            }
        }

        public double TheoreticalSwitchWinProbability(int boxCount)
        {
            return (double)(boxCount - 1) / boxCount;
        }
    }
}
