
using System.Net;



public class RoomServer
{

    private IPAddress ipAdress;
    private int portNumber;
    private RoomNetworkManager networkManager;

    public RoomServer(IPAddress ipAdress, int portNumber)
    {
        this.ipAdress = ipAdress;
        this.portNumber = portNumber;
    }

    public void StartRoomServer()
    {
        networkManager = new();
        networkManager.StartListen(ipAdress, portNumber);
        networkManager.OnDataReceived += HandleRecevieData;
    }

    private void HandleRecevieData(byte[] data)
    {

    }
}

