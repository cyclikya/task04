namespace Randm.API
{

    public interface IMorty
    {
        string Name { get; }

        void Initialize(IGameHost host);

        int ChooseHideBox(int boxCount);

        int[] ChooseBoxesToKeep(int boxCount, int rickInitialChoice, int hiddenIndex);

        double TheoreticalSwitchWinProbability(int boxCount);
    }
}
