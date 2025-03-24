using Google.Protobuf.WellKnownTypes;
using MsgTest;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PacketSender : MonoBehaviour
{
    private static PacketSender _instance;
    public static PacketSender Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PacketSender>();
            }
            return _instance;
        }
    }

    public async void RequestLogin()
    {
        string id = UIManager.Instance.text_Id.text;
        string pw = UIManager.Instance.text_pw.text;

        MsgTest.CS_Login_Request sendLoginRequestPacket = new MsgTest.CS_Login_Request();
        sendLoginRequestPacket.Id = id;
        sendLoginRequestPacket.Password = pw;

        Debug.Log("[Request Login] ID : " + sendLoginRequestPacket.Id + " PW : " + sendLoginRequestPacket.Password);


        var responseTask = ServerConnector.Instance.WaitForAsyncResponse(PacketId.PKT_SC_LOGIN_RESPONSE);

        byte[] sendBuffer = PacketHandler.MakeSendData(PacketId.PKT_CS_LOGIN_REQUEST, sendLoginRequestPacket);
        ServerConnector.Instance.Send(sendBuffer, sendBuffer.Length);

        byte[] recvData = await responseTask;
        MsgTest.SC_Login_Response recvLoginResponsePacket;
        recvLoginResponsePacket = MsgTest.SC_Login_Response.Parser.ParseFrom(recvData);

        if(recvLoginResponsePacket.Success == false)
        {
            Debug.Log("[LOGIN FAIL] Error Message : " + recvLoginResponsePacket.ErrorMessage);
            return;
        }

        UInt64 sessionId = recvLoginResponsePacket.SessionId;

        GameManager.Instance._playerInfo.SetUserId(sessionId);

        Debug.Log("[LOGIN SUCCESS] SessionId : " + sessionId);

        // Request User Info
        MsgTest.CS_My_Player_Info_Request sendMyPlayerInfoRequestPacket;
        sendMyPlayerInfoRequestPacket = new CS_My_Player_Info_Request();

        sendMyPlayerInfoRequestPacket.SessionId = sessionId;

        responseTask = ServerConnector.Instance.WaitForAsyncResponse(PacketId.PKT_SC_MY_PLAYER_INFO_RESPONSE);

        sendBuffer = PacketHandler.MakeSendData(PacketId.PKT_CS_MY_PLAYER_INFO_REQUEST, sendMyPlayerInfoRequestPacket);
        ServerConnector.Instance.Send(sendBuffer, sendBuffer.Length);

        recvData = await responseTask;
        MsgTest.SC_My_Player_Info_Response recvMyPlayerinfoResponsePacket;
        recvMyPlayerinfoResponsePacket = MsgTest.SC_My_Player_Info_Response.Parser.ParseFrom(recvData);

        MsgTest.Player player = recvMyPlayerinfoResponsePacket.PlayerInfo;
        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.SetName(player.Name);
        playerInfo.SetLevel(player.Level);
        playerInfo.SetPosition(new Vector3(player.Position.X, player.Position.Y, player.Position.Z));
        GameManager.Instance.SetPlayerInfo(playerInfo);
    }

    public async void RequestRoomList()
    {
        MsgTest.CS_Room_List_Request sendRoomListRequestPacket;
        sendRoomListRequestPacket = new MsgTest.CS_Room_List_Request();

        var responseTask = ServerConnector.Instance.WaitForAsyncResponse(PacketId.PKT_SC_ROOM_LIST_RESPONSE);

        byte[] sendBuffer = PacketHandler.MakeSendData(PacketId.PKT_CS_ROOM_LIST_REQUEST, sendRoomListRequestPacket);
        ServerConnector.Instance.Send(sendBuffer, sendBuffer.Length);

        byte[] recvData = await responseTask;
        MsgTest.SC_Room_List_Response recvRoomListResponsePacket;
        recvRoomListResponsePacket = MsgTest.SC_Room_List_Response.Parser.ParseFrom(recvData);

        List<Room> roomList = new List<Room>();
        foreach (MsgTest.Room room in recvRoomListResponsePacket.RoomList)
        {
            roomList.Add(new Room(room.RoomId, room.RoomName, room.MaxPlayerCount, room.PlayerCount, room.HostPlayerName));
        }

        Debug.Log("[RECV ROOM LIST] Room Count : " + roomList.Count);

        UIManager.Instance.UpdateRoomList(roomList);
    }

    public async void RequestCreateRoom()
    {
        string roomName = UIManager.Instance.inputField_createRoomName.text;
        string hostName = GameManager.Instance._playerInfo.GetName();

        MsgTest.CS_Create_Room_Request sendCreateRoomPacket;
        sendCreateRoomPacket = new MsgTest.CS_Create_Room_Request();

        sendCreateRoomPacket.RoomName = roomName;
        sendCreateRoomPacket.HostName = hostName;

        var responseTask = ServerConnector.Instance.WaitForAsyncResponse(PacketId.PKT_SC_CREATE_ROOM_RESPONSE);

        byte[] sendBuffer = PacketHandler.MakeSendData(PacketId.PKT_CS_CREATE_ROOM_REQUEST, sendCreateRoomPacket);
        ServerConnector.Instance.Send(sendBuffer, sendBuffer.Length);

        byte[] recvData = await responseTask;
        MsgTest.SC_Create_Room_Response recvCreateRoomResponsePacket;
        recvCreateRoomResponsePacket = MsgTest.SC_Create_Room_Response.Parser.ParseFrom(recvData);


        MsgTest.Room room = recvCreateRoomResponsePacket.Room;
        if (recvCreateRoomResponsePacket.Success == false)
        {
            Debug.Log("[RECV Create Room Fail] Error Message : " + recvCreateRoomResponsePacket.ErrorMessage);
            return;
        }
        else
        {
            Debug.Log("[RECV Create Room Success] Room ID : " + room.RoomId);
        }

        Int32 roomId = room.RoomId;

        Room roomInfo;
        roomInfo._roomId = roomId;
        roomInfo._roomName = roomName;
        roomInfo._hostPlayerName = hostName;
        roomInfo._playerCount = room.PlayerCount;
        roomInfo._maxPlayerCount = room.MaxPlayerCount;

        // Enter Room
        UIManager.Instance.ActivateRoomList();
        GameManager.Instance.EnterRoom(roomInfo);

    }

    public async void JoinRoom(Int32 roomId)
    {
        // request enter room
        {
            MsgTest.CS_Enter_Room_Request sendEnterRoomRequestPacket;
            sendEnterRoomRequestPacket = new CS_Enter_Room_Request();

            sendEnterRoomRequestPacket.RoomId = roomId;

            var responseTask = ServerConnector.Instance.WaitForAsyncResponse(PacketId.PKT_SC_ENTER_ROOM_RESPONSE);

            byte[] sendBuffer = PacketHandler.MakeSendData(PacketId.PKT_CS_ENTER_ROOM_REQUEST, sendEnterRoomRequestPacket);
            ServerConnector.Instance.Send(sendBuffer, sendBuffer.Length);

            byte[] recvData = await responseTask;

            MsgTest.SC_Enter_Room_Response recvEnterRoomResponsePacket;
            recvEnterRoomResponsePacket = MsgTest.SC_Enter_Room_Response.Parser.ParseFrom(recvData);
            MsgTest.Room room = recvEnterRoomResponsePacket.Room;

            if (recvEnterRoomResponsePacket.Success == false)
            {
                Debug.Log("[ENTER ROOM FAIL] error message : " + recvEnterRoomResponsePacket.ErrorMessage);
                return;
            }
            else
            {
                Debug.Log("[ENTER ROOM SUCCESS] room Name : " + room.RoomName);
            }
            Room roomInfo = new Room();
            roomInfo._roomId = room.RoomId;
            roomInfo._roomName = room.RoomName;
            roomInfo._playerCount = room.PlayerCount;
            roomInfo._maxPlayerCount = room.MaxPlayerCount;
            roomInfo._hostPlayerName = room.HostPlayerName;

            GameManager.Instance.EnterRoom(roomInfo);
        }
        
        // request user list
        {
            MsgTest.CS_Room_Player_List_Request sendRoomPlayerListRequestPacket;
            sendRoomPlayerListRequestPacket = new CS_Room_Player_List_Request();

            sendRoomPlayerListRequestPacket.RoomId = roomId;

            var responseTask = ServerConnector.Instance.WaitForAsyncResponse(PacketId.PKT_SC_ROOM_PLAYER_LIST_RESPONSE);

            byte[] sendBuffer = PacketHandler.MakeSendData(PacketId.PKT_CS_ROOM_PLAYER_LIST_REQUEST, sendRoomPlayerListRequestPacket);
            ServerConnector.Instance.Send(sendBuffer, sendBuffer.Length);

            byte[] recvData = await responseTask;

            MsgTest.SC_Room_Player_List_Response recvRoomPlayerListResponsePacket;
            recvRoomPlayerListResponsePacket = MsgTest.SC_Room_Player_List_Response.Parser.ParseFrom(recvData);

            Debug.Log("[RECV ROOM PLAYER LIST] player count : " + recvRoomPlayerListResponsePacket.PlayerList.Count);
            foreach(MsgTest.Player player in recvRoomPlayerListResponsePacket.PlayerList)
            {
                if(player.Name != GameManager.Instance._playerInfo.GetName())
                {
                    Vector3 playerPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z);
                    
                    PlayerInfo playerInfo = new PlayerInfo();
                    playerInfo.SetName(player.Name);
                    playerInfo.SetLevel(player.Level);
                    playerInfo.SetPosition(playerPosition);

                    GameManager.Instance.PlayerJoinedRoom(playerInfo);
                }
            }
        }
    }

    public void SendPlayerPosition(Vector3 position, Vector3 velocity)
    {
        MsgTest.CS_Player_Move_Request sendPlayerMoveRequestPacket;
        sendPlayerMoveRequestPacket = new CS_Player_Move_Request();
        sendPlayerMoveRequestPacket.MoveState = new MsgTest.MoveState();
        sendPlayerMoveRequestPacket.MoveState.Position = new MsgTest.Vector();
        sendPlayerMoveRequestPacket.MoveState.Velocity = new MsgTest.Vector();

        sendPlayerMoveRequestPacket.MoveState.PlayerName = GameManager.Instance._playerInfo.GetName();
        sendPlayerMoveRequestPacket.MoveState.Position.X = position.x;
        sendPlayerMoveRequestPacket.MoveState.Position.Y = position.y;
        sendPlayerMoveRequestPacket.MoveState.Position.Z = position.z;
        sendPlayerMoveRequestPacket.MoveState.Velocity.X = velocity.x;
        sendPlayerMoveRequestPacket.MoveState.Velocity.Y = velocity.y;
        sendPlayerMoveRequestPacket.MoveState.Velocity.Z = velocity.z;
        sendPlayerMoveRequestPacket.MoveState.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        byte[] sendBuffer = PacketHandler.MakeSendData(PacketId.PKT_CS_PLAYER_MOVE_REQUEST, sendPlayerMoveRequestPacket);
        
        ServerConnector.Instance.Send(sendBuffer, sendBuffer.Length);
    }
}
