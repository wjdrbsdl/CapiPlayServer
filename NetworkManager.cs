using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public enum HeadType
{
    MapData,
}


public class NetworkManager
{
    public static IPAddress ServerIp;
    static int index = 5;
    public static int bufferSize = 2;
    Socket mainSock;
    List<AsyncObject> connectedClientList = new List<AsyncObject>();
    int m_port = 5000;
    public static byte failCode = 255;

    public void Connect()
    {
        Console.WriteLine("서버 연결 시작" + ParseCurIP.GetLocalIP());
        ServerIp = IPAddress.Parse(ParseCurIP.GetLocalIP());
        mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, m_port);
        mainSock.Bind(serverEP);
        mainSock.Listen(10);
        mainSock.BeginAccept(AcceptCallback, null);
    }

    void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            Console.WriteLine("서버 수락 콜백 - 클라로부터 받음");
            Socket client = mainSock.EndAccept(ar);
            AsyncObject obj = new AsyncObject(bufferSize);
            obj.numbering = index;
            index++;
            obj.WorkingSocket = client;
            connectedClientList.Add(obj);
            client.BeginReceive(obj.Buffer, 0, bufferSize, 0, DataReceived, obj);


            mainSock.BeginAccept(AcceptCallback, index);
        }
        catch
        {
            Console.WriteLine("받아들이기 실패");
        }
    }

    void DataReceived(IAsyncResult ar)
    {
        try
        {
            Console.WriteLine("데이터 뭘 받음");
            AsyncObject asyObj = (AsyncObject)ar.AsyncState;

            byte[] msgLengthBuff = asyObj.Buffer;
            
            ushort msgLength = EndianChanger.NetToHost(msgLengthBuff);
            byte[] recvBuffer = new byte[msgLength];
            byte[] recvData = new byte[msgLength];
            int recv = 0;
            int recvIdx = 0;
            int rest = msgLength;

            while (rest > 0)
            {
                int toRead = Math.Min(rest, recvBuffer.Length);
                recv = asyObj.WorkingSocket.Receive(recvBuffer, 0, toRead, SocketFlags.None);

                if (recv == 0)
                {
                    Console.WriteLine("상대방 연결 종료");
                    AddRemoveSokect(asyObj.numbering);
                    return;
                }

                Buffer.BlockCopy(recvBuffer, 0, recvData, recvIdx, recv);
                recvIdx += recv;
                rest -= recv;
            }

            Console.WriteLine("요청 받음 " + (HeadType)recvData[0]);

            HandleRoomMaker(asyObj, recvData);

            if (asyObj.WorkingSocket.Connected)
                asyObj.WorkingSocket.BeginReceive(asyObj.Buffer, 0, asyObj.BufferSize, 0, DataReceived, asyObj);
        }
        catch
        {

            AsyncObject obj = (AsyncObject)ar.AsyncState;
            AddRemoveSokect(obj.numbering);
            Console.WriteLine("서버에서이상 접속한 소켓 수" + connectedClientList.Count);
        }

    }

    public class AsyncObject
    {
        public int numbering;
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[(long)BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }

    void HandleRoomMaker(AsyncObject _obj, byte[] data)
    {
        //
        HeadType reqType = (HeadType)data[0];
        if (reqType == HeadType.MapData)
        {
           MakeMapdate(data);
        }
    }

    private void MakeMapdate(byte[] data)
    {
        List<GameMarkerData> markers = new List<GameMarkerData>();
        int index = 1;
        // 마커 수
        int markerCount = data[index++];

        for (int i = 0; i < markerCount; i++)
        {
            GameMarkerData marker = new GameMarkerData();

            marker.markId = BitConverter.ToInt32(data, index);
            index += 4;

            marker.dropItemId = BitConverter.ToInt32(data, index);
            index += 4;

            marker.markerSpawnType = (MarkerSpawnType)data[index++];
            marker.markerType = (MarkerType)data[index++];

            // 문자열 (길이 + UTF-8 문자열)
            byte nameLength = data[index++];
            marker.name = System.Text.Encoding.UTF8.GetString(data, index, nameLength);
            index += nameLength;

            marker.spawnStep = BitConverter.ToInt32(data, index);
            index += 4;

            marker.deleteStep = BitConverter.ToInt32(data, index);
            index += 4;

            // Vector3 position
            marker.positionX = BitConverter.ToSingle(data, index); index += 4;
            marker.positionY = BitConverter.ToSingle(data, index); index += 4;
            marker.positionZ = BitConverter.ToSingle(data, index); index += 4;

            // Vector3 rotation
            marker.rotationX = BitConverter.ToSingle(data, index); index += 4;
            marker.rotationY = BitConverter.ToSingle(data, index); index += 4;
            marker.rotationZ = BitConverter.ToSingle(data, index); index += 4;

            markers.Add(marker);
            Console.WriteLine($"{marker.name}생성 x{marker.positionX}:{marker.positionY}:{marker.positionZ}");
        }

    }

    private void SendData(AsyncObject _obj, byte[] _msg)
    {
        //헤더작업 용량 길이 붙여주기 
        ushort msgLength = (ushort)_msg.Length;
        byte[] msgLengthBuff = new byte[2];
        msgLengthBuff = BitConverter.GetBytes(msgLength);

        byte[] originPacket = new byte[msgLengthBuff.Length + msgLength];
        Buffer.BlockCopy(msgLengthBuff, 0, originPacket, 0, msgLengthBuff.Length); //패킷 0부터 메시지 길이 버퍼 만큼 복사
        Buffer.BlockCopy(_msg, 0, originPacket, msgLengthBuff.Length, msgLength); //패킷 메시지길이 버퍼 길이 부터, 메시지 복사

        int rest = (msgLength + msgLengthBuff.Length);
        int send = 0;
        do
        {
            byte[] sendPacket = new byte[rest];
            Buffer.BlockCopy(originPacket, originPacket.Length - rest, sendPacket, 0, rest);
            send = _obj.WorkingSocket.Send(sendPacket);
            rest -= send;
        } while (rest >= 1);

    }


    Queue<int> removeQueue = new Queue<int>();
    private void AddRemoveSokect(int _numbering)
    {
        removeQueue.Enqueue(_numbering);
    }
}


public class EndianChanger
{

    public static ushort NetToHost(byte[] _bigEndians)
    {
        return (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(_bigEndians));
    }

    public static byte[] HostToNet(ushort _length)
    {
        byte[] lengthByte = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)_length));
        return lengthByte;
    }
}

public class ParseCurIP
{
    public static string GetLocalIP()
    {
        string result = string.Empty;

        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                result = ip.ToString();
                return result;
            }


        }
        return null;
    }
}
