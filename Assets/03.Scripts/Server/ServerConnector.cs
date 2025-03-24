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

    public string serverIP = "127.0.0.1"; // ���� IP �ּ�
    public int serverPort = 7777; // ���� ��Ʈ ��ȣ

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

    // ���� ����
    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            isConnected = true;

            Debug.Log("Connected to server!");

            // �������� ���� �޽����� �����ϴ� ������ ����
            receiveThread = new Thread(Recv);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
        }
    }

    // ������ �޽��� ����
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

    // �������� �޽��� ����
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
            throw new InvalidOperationException("�̹� ��� ���� ���� ��û�� �ֽ��ϴ�.");
        }

        _asyncRecvTask = new TaskCompletionSource<byte[]>();
        _registeredAsyncPacketId = responsePacketId;
        _registeredPacketRecv = true;

        var result = await _asyncRecvTask.Task;
        _asyncRecvTask = null; // �Ϸ� �� ����
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


