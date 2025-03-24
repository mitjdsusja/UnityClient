using System;
using TMPro;
using UnityEngine;

public class RoomButtonUI : MonoBehaviour
{
    public TextMeshProUGUI text_roomName;
    public TextMeshProUGUI text_playerCount;
    public TextMeshProUGUI text_hostPlayerName;

    const string stringRoomName = "RoomName : ";
    const string stringPlayerCount = "player : ";
    const string stringHostPlayerName = "HOST : ";

    Room _roomInfo;

    private void Awake()
    {
        text_roomName.text = stringRoomName + "NULL";
        text_playerCount.text = stringPlayerCount + "NULL";
        text_hostPlayerName.text = stringHostPlayerName + "NULL";
    }

    public void SetRoomInfo(Room room)
    {
        _roomInfo = room;
        text_roomName.text = stringRoomName + room._roomName;
        text_playerCount.text = stringPlayerCount + room._playerCount + " / " + room._maxPlayerCount;
        text_hostPlayerName.text = stringHostPlayerName + room._hostPlayerName;
    }

    public void JoinRoom()
    {
        PacketSender.Instance.JoinRoom(_roomInfo._roomId);
    }
}
