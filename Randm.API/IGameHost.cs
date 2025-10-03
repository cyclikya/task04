namespace Randm.API
{

    public interface IGameHost
    {

        int RequestFairRandom(int n, string purpose);

        void Log(string message);
    }
}
