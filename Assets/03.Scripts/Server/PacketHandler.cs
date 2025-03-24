using System;
using UnityEngine;

using Google.Protobuf;
using MsgTest;
using System.Net;
using Unity.VisualScripting;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading.Tasks;

struct PacketHeader
{
    public int packetSize;
    public int packetId;
}

public enum PacketId
{

    /*------------
		C -> S
	-------------*/
    // 1000 ~ 1099
    PKT_CS_1000 = 1000,
    PKT_CS_LOGIN_REQUEST = 1001,

    // 1100 ~ 1199
    PKT_CS_ROOM_LIST_REQUEST = 1101,
    PKT_CS_MY_PLAYER_INFO_REQUEST = 1102,
    PKT_CS_ROOM_PLAYER_LIST_REQUEST = 1103,

    // 1200 ~ 
    PKT_CS_1200 = 1200,
    PKT_CS_ENTER_ROOM_REQUEST = 1201,
    PKT_CS_CREATE_ROOM_REQUEST = 1202,
    PKT_CS_PLAYER_MOVE_REQUEST = 1203,


    /*------------
		S -> C
	-------------*/
    // 2000 ~ 
    PKT_SC_2000 = 2000,
    PKT_SC_LOGIN_RESPONSE = 2001,

    // 2100 ~ 2199
    PKT_SC_2100 = 2100,
    PKT_SC_ROOM_LIST_RESPONSE = 2101,
    PKT_SC_MY_PLAYER_INFO_RESPONSE = 2102,
    PKT_SC_ROOM_PLAYER_LIST_RESPONSE = 2103,
    PKT_SC_ENTER_ROOM_RESPONSE = 2104,
    PKT_SC_CREATE_ROOM_RESPONSE = 2105,
    PKT_SC_PLAYER_ENTER_ROOM_NOTIFICATION = 2106,
    PKT_SC_PLAYER_MOVE_NOTIFICATION = 2107,

};

public class PacketHandler
{
    private static Dictionary<PacketId, Action<byte[], int>> _packetProcess;

    public static void InitPacketHandler()
    {
        _packetProcess = new Dictionary<PacketId, Action<byte[], int>>();

        _packetProcess[PacketId.PKT_SC_PLAYER_ENTER_ROOM_NOTIFICATION] = Handle_SC_Player_Enter_Room_Notification;
        _packetProcess[PacketId.PKT_SC_PLAYER_MOVE_NOTIFICATION] = Handle_SC_Player_Move_Notification;
    }
    public static void HandlePacket(byte[] buffer)
    {
        PacketHeader header = new PacketHeader
        {
            packetSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0)),
            packetId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, sizeof(int)))
        };
        //Debug.Log("[RECV] PacketId : " + header.packetId + " PacketSize : " + header.packetSize);

        int dataStartIndex = sizeof(int) * 2;
        int dataSize = header.packetSize - dataStartIndex;

        byte[] payload = new byte[dataSize];
        Array.Copy(buffer, dataStartIndex, payload, 0, dataSize);

        // 예약해놓은 Packet이 왔다면
        if (ServerConnector.Instance._registeredPacketRecv == true 
            && ServerConnector.Instance._registeredAsyncPacketId == (PacketId)header.packetId)
        {
            ServerConnector.Instance.HandleAsyncResponse(payload);
            return;
        }

        // 일반적인 패킷 처리 (Unity 메인 스레드에서 실행)
        UnityMainThreadDispatcher.Enqueue(() => {
            _packetProcess[(PacketId)header.packetId](payload, dataSize);
        });
    }

    public static byte[] MakeSendData<T>(PacketId packetId, T data) where T : IMessage
    {
        byte[] protobufData = data.ToByteArray();
        int dataSize = protobufData.Length;

        byte[] sendData = new byte[dataSize + sizeof(int) * 2]; // dataSize + PacketHeader

        int packetSize = sizeof(int) * 2 + dataSize;

        WriteInt32BigEndian(packetSize, sendData, 0);
        WriteInt32BigEndian((int)packetId, sendData, sizeof(int));

        protobufData.CopyTo(sendData, sizeof(int) * 2);

        return sendData;
    }

    public static void WriteInt32BigEndian(int value, byte[] buffer, int offset)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        Array.Copy(bytes, 0, buffer, offset, sizeof(int));
    }

    //-----------------HandlePacketProcess-------------------//

    public static void Handle_Invalid_Packet(byte[] data, int dataSize)
    {
        Debug.Log("RECV INVALID PACKET ID");
    }

    public static void Handle_SC_Player_Enter_Room_Notification(byte[] data, int dataSize)
    {
        MsgTest.SC_Player_Enter_Room_Notification recvPlayerEnterRoomNotificationPacket;
        recvPlayerEnterRoomNotificationPacket = MsgTest.SC_Player_Enter_Room_Notification.Parser.ParseFrom(data);
        MsgTest.Player player = recvPlayerEnterRoomNotificationPacket.Player;

        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.SetName(player.Name);
        playerInfo.SetLevel(player.Level);
        playerInfo.SetPosition(new Vector3(player.Position.X, player.Position.Y, player.Position.Z));

        GameManager.Instance.PlayerJoinedRoom(playerInfo);
        Debug.Log("[RECV PLAYER ENTER ROOM] enter player name : " +  player.Name);
    }

    public static void Handle_SC_Player_Move_Notification(byte[] data, int dataSize) {
        MsgTest.SC_Player_Move_Notification recvPlayerMoveNotificationPacket;
        recvPlayerMoveNotificationPacket = MsgTest.SC_Player_Move_Notification.Parser.ParseFrom(data);
        foreach (MsgTest.MoveState moveState in recvPlayerMoveNotificationPacket.MoveStates)
        {
            string name = moveState.PlayerName;
            Vector3 position = new Vector3(moveState.Position.X, moveState.Position.Y, moveState.Position.Z);
            Vector3 velocity = new Vector3(moveState.Velocity.X, moveState.Velocity.Y, moveState.Velocity.Z);
            Int64 timestamp = moveState.Timestamp;
            GameManager.Instance.PlayerMove(name, position, velocity, timestamp);
        }
    }
}
