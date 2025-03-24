using MsgTest;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{


    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<UIManager>();
            }
            return _instance;
        }
    }


    // PlayerInfo
    [SerializeField] TextMeshProUGUI text_playerName;
    [SerializeField] TextMeshProUGUI text_playerLevel;
    [SerializeField] TextMeshProUGUI text_playerPosX;
    [SerializeField] TextMeshProUGUI text_playerPosY;
    [SerializeField] TextMeshProUGUI text_playerPosZ;

    // RoomId
    [SerializeField] private TextMeshProUGUI text_RoomId;

    // RoomList UI
    public RoomListUI roomListUI;

    // Login Text UI
    public TMP_InputField text_Id;
    public TMP_InputField text_pw;

    public TMP_InputField inputField_createRoomName;

    public void ActivateRoomList()
    {
        roomListUI.ActivateRoomList();
    }
    public void UpdateRoomList(List<Room> roomList)
    {
        GameManager.Instance._rooms = roomList;

        // UI
        roomListUI.UpdateRoomListUI(roomList);
    }
    public void UpdateRoomInfo(Room roomInfo)
    {
        text_RoomId.text = "Room ID : " + roomInfo._roomId;
    }
    public void UpdatePlayerInfo(PlayerInfo playerInfo)
    {
        text_playerName.text = "name : " + playerInfo.GetName();
        text_playerLevel.text = "level : " + playerInfo.GetLevel().ToString();
        text_playerPosX.text = "posX : " + playerInfo.GetPosition().x.ToString();
        text_playerPosY.text = "posY : " + playerInfo.GetPosition().y.ToString();
        text_playerPosZ.text = "posZ : " + playerInfo.GetPosition().z.ToString();
    }
}
