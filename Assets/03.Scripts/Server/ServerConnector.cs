using System.Net.Sockets;
using System.Threading;
using System;
using UnityEngine;
using System.Threading.Tasks;

public class ServerConnector : MonoBehaviour
{
    private static ServerConnector _instance;
    public static ServerConnector Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<ServerConnector>();
            }
            return _instance;
        }
    }

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    public string serverIP = "127.0.0.1"; // 서버 IP 주소
    public int serverPort = 7777; // 서버 포트 번호

    public PacketId _registeredAsyncPacketId = 0;
    public bool _registeredPacketRecv = false;
    public TaskCompletionSource<byte[]> _asyncRecvTask = null;

    void Start()
    {
        _instance = GetComponent<ServerConnector>();
        PacketHandler.InitPacketHandler();

    }

    void Update()
    {
       
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    // 서버 연결
    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            isConnected = true;

            Debug.Log("Connected to server!");

            // 서버에서 오는 메시지를 수신하는 쓰레드 시작
            receiveThread = new Thread(Recv);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
        }
    }

    // 서버로 메시지 전송
    public void Send(byte[] data, int dataSize)
    {
        try
        {
            if (stream != null)
            {
                stream.Write(data, 0, dataSize);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending message: {e.Message}");
        }
    }

    // 서버에서 메시지 수신
    private void Recv()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (isConnected)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Disconnect();
                }

                PacketHandler.HandlePacket(buffer);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving message: {e.Message}");
        }
    }

    public async Task<byte[]> WaitForAsyncResponse(PacketId responsePacketId)
    {
        if (_asyncRecvTask != null)
        {
            throw new InvalidOperationException("이미 대기 중인 동기 요청이 있습니다.");
        }

        _asyncRecvTask = new TaskCompletionSource<byte[]>();
        _registeredAsyncPacketId = responsePacketId;
        _registeredPacketRecv = true;

        var result = await _asyncRecvTask.Task;
        _asyncRecvTask = null; // 완료 후 정리
        return result;
    }
    public void HandleAsyncResponse(byte[] data)
    {
        _registeredAsyncPacketId = 0;
        _registeredPacketRecv = false;

        _asyncRecvTask.TrySetResult(data);
    }

    public void Disconnect()
    {
        try
        {
            isConnected = false;
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Abort();
            }
            if (stream != null)
            {
                stream.Close();
            }
            if (client != null)
            {
                client.Close();
            }

            Debug.Log("Disconnected from server.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error disconnecting: {e.Message}");
        }
    }
}


