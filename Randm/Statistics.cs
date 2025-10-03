using System;

namespace Randm
{
    public class Statistics
    {
        public int RoundsSwitched { get; private set; }
        public int RoundsStayed { get; private set; }
        public int WinsWhenSwitched { get; private set; }
        public int WinsWhenStayed { get; private set; }
        public double TheoreticalSwitchProbability { get; private set; } = 0.0;

        public void RegisterRound(bool switched, bool rickWon, double theoreticalSwitchProbability)
        {
            TheoreticalSwitchProbability = theoreticalSwitchProbability;
            if (switched)
            {
                RoundsSwitched++;
                if (rickWon) WinsWhenSwitched++;
            }
            else
            {
                RoundsStayed++;
                if (rickWon) WinsWhenStayed++;
            }
        }

        public double EstimatedSwitchWinProbability()
        {
            return RoundsSwitched == 0 ? 0.0 : (double)WinsWhenSwitched / RoundsSwitched;
        }
        public double EstimatedStayWinProbability()
        {
            return RoundsStayed == 0 ? 0.0 : (double)WinsWhenStayed / RoundsStayed;
        }
    }
}
