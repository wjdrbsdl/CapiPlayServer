
using System.Net;
using System.Net.Sockets;


public class RoomInfo{
    public int roomSerialNumber;
    public int mapNumber;
    public ServerMapData mapData;
}

public class RoomNetworkManager
{
    public event Action<byte[]> OnDataReceived;
    public event Action OnConnected;

    private List<Socket> userSocketList;
    private Socket listenSocket;

  
    public void StartListen(IPAddress ipAdress, int port)
    {
        userSocketList = new();
        listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, port);
        listenSocket.Bind(serverEP);
        listenSocket.Listen(5);
        listenSocket.BeginAccept(AcceptCallBack, null);
    }


    private void AcceptCallBack(IAsyncResult  result)
    {
        Console.WriteLine("방에서 손님 받음");
        Socket clientSocket = listenSocket.EndAccept(result); // 클라이언트 소켓 수락
        userSocketList.Add(clientSocket); // 소켓 목록에 추가

        // 다음 클라이언트 수신을 위해 다시 BeginAccept 호출
        listenSocket.BeginAccept(AcceptCallBack, null);

        // 클라이언트 소켓 수신 시작
        BeginReceive(clientSocket);
    }

    private class ReceiveState
    {
        public Socket Socket;
        public byte[] Buffer;
        public int TotalExpected; // 전체 기대하는 길이
        public int TotalReceived; // 지금까지 받은 길이
        public bool IsReceivingBody; // true면 본문, false면 헤더
    }

    private void BeginReceive(Socket clientSocket)
    {
        ReceiveState state = new ReceiveState
        {
            Socket = clientSocket,
            Buffer = new byte[2],
            TotalExpected = 2,
            TotalReceived = 0,
            IsReceivingBody = false
        };

        clientSocket.BeginReceive(state.Buffer, 0, 2, SocketFlags.None, ReceiveCallback, state);
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            Console.WriteLine("룸서버 응답");
            var state = (ReceiveState)result.AsyncState;
            Socket clientSocket = state.Socket;

            int bytesRead = clientSocket.EndReceive(result);
            if (bytesRead == 0)
            {
                // 연결 종료됨
                return;
            }

            state.TotalReceived += bytesRead;

            if (state.TotalReceived < state.TotalExpected)
            {
                // 아직 다 못 받았으면 나머지를 이어서 받음
                clientSocket.BeginReceive(
                    state.Buffer,
                    state.TotalReceived,
                    state.TotalExpected - state.TotalReceived,
                    SocketFlags.None,
                    ReceiveCallback,
                    state);
                return;
            }

            if (!state.IsReceivingBody)
            {
                // 헤더 수신 완료 → 메시지 길이 읽고 본문 수신 시작
                ushort msgLength = EndianChanger.NetToHost(state.Buffer);
                state.Buffer = new byte[msgLength];
                state.TotalExpected = msgLength;
                state.TotalReceived = 0;
                state.IsReceivingBody = true;

                clientSocket.BeginReceive(
                    state.Buffer,
                    0,
                    msgLength,
                    SocketFlags.None,
                    ReceiveCallback,
                    state);
            }
            else
            {
                // 본문 수신 완료
                OnDataReceived?.Invoke(state.Buffer);

                // 다시 다음 메시지 수신 (헤더부터)
                BeginReceive(clientSocket);
            }
        }
        catch
        {
            // 연결 실패 또는 중단 처리
        }
    }

    public void Send(byte[] data)
    {
        // DebugManager.instance.EnqueMessege("매니저에서 샌드");
        ushort length = (ushort)data.Length;
        byte[] header = EndianChanger.HostToNet(length);
        byte[] packet = new byte[header.Length + data.Length];
        Buffer.BlockCopy(header, 0, packet, 0, header.Length);
        Buffer.BlockCopy(data, 0, packet, header.Length, data.Length);
        //clientSocket.Send(packet);


        int totalSent = 0;
        int totalLength = packet.Length;

        while (totalSent < totalLength)
        {
            int sent = listenSocket.Send(packet, totalSent, totalLength - totalSent, SocketFlags.None);
            //  UnityEngine.Debug.Log($"{totalLength} 중 : {sent}만큼 보냄");
            if (sent == 0)
            {
                Console.WriteLine("데이터 전송 중 상대방 연결 종료");
                return;
            }
            totalSent += sent;
        }
    }

    public void Disconnect()
    {
        listenSocket?.Close();
        listenSocket?.Dispose();
    }
}
