using MsgTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomListUI : MonoBehaviour
{
    // Room Button Prefab
    public GameObject _prefabRoomButton;

    // UI
    public Transform _parentRoomButton;
    public List<GameObject> _roomButtonObjectList;

    public Int32 _curRoomId = 0;

    public void ActivateRoomList()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void UpdateRoomListUI(List<Room> roomList)
    {
        foreach(GameObject room in _roomButtonObjectList)
        {
            Destroy(room);
        }

        foreach(Room room in roomList)
        {
            GameObject roomItem = Instantiate(_prefabRoomButton, _parentRoomButton);
            RoomButtonUI roomUIScript = roomItem.GetComponent<RoomButtonUI>();
            roomUIScript.SetRoomInfo(room);
            /* room data set
             * 
             */

            _roomButtonObjectList.Add(roomItem);
        }
        
    }

    private void Start()
    {
        _roomButtonObjectList = new List<GameObject>();

        gameObject.SetActive(false);
    }
}
