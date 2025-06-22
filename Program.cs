namespace CapibaraServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LobbyServer netManger = new();

            netManger.Connect();

            while (true)
            {

            }
        }
    }
}