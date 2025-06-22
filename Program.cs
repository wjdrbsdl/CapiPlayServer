namespace CapibaraServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NetworkManager netManger = new();

            netManger.Connect();

            while (true)
            {

            }
        }
    }
}