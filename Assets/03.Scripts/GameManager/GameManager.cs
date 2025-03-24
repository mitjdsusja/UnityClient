using MsgTest;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameManager>();
            }
            return _instance;
        }
    }

    // Prefabs
    public GameObject _otherPlayerPrefabs;

    public GameObject _playerObject;
    Dictionary<string, GameObject> _otherPlayersObject;

    // Player struct
    public PlayerInfo _playerInfo;
    public Dictionary<string, PlayerInfo> _playersInfo;

    // Room Sturct
    public bool _isRoomEnter = false;

    public List<Room> _rooms;
    private Room _curRoom;

    void Start()
    {
        _instance = this;
        _playersInfo = new Dictionary<string, PlayerInfo>();
        _otherPlayersObject = new Dictionary<string, GameObject>();
    }
    public void EnterRoom(Room roomInfo)
    {
        _curRoom = roomInfo;
        UIManager.Instance.UpdateRoomInfo(_curRoom);

        _isRoomEnter = true;
    }

    public void PlayerJoinedRoom(PlayerInfo playerInfo)
    {
        if(playerInfo.GetName() == _playerInfo.GetName())
        {
            return; 
        }

        _playersInfo.Add(playerInfo.GetName(), playerInfo);

        GameObject otehrPlayerObject = GameObject.Instantiate(_otherPlayerPrefabs, playerInfo.GetPosition(), Quaternion.identity);
        OtherPlayerMove playerMove = otehrPlayerObject.GetComponent<OtherPlayerMove>();
        playerMove.SetPlayerInfoKey(playerInfo.GetName());

        _otherPlayersObject.Add(playerInfo.GetName(), otehrPlayerObject);
    }
    public void SetPlayerInfo(PlayerInfo playerInfo)
    {
        _playerInfo = playerInfo;
        UIManager.Instance.UpdatePlayerInfo(playerInfo);
    }

    public void PlayerMove(string name, Vector3 position, Vector3 velocity, Int64 timestamp)
    {

        if (name == _playerInfo.GetName())
        {
            return;
        }
        if (_playersInfo.ContainsKey(name) == false)
        {
            Debug.Log("[INVALID PLAYER] player name : " + name);
            return;
        }

        PlayerInfo playerInfo = _playersInfo[name];
        playerInfo.SetPosition(position);
        playerInfo.SetVelocity(velocity);
        playerInfo.SetLastMovePacketTime(timestamp);
    }

    public PlayerInfo GetPlayerInfo(string playerName)
    {
        if (_playersInfo.ContainsKey(playerName) == false)
        {
            return null;
        }

        PlayerInfo playerInfo = _playersInfo[playerName];
        return playerInfo;
    }
}
