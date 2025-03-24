using System;
using UnityEngine;

public struct Room
{
    public Int32 _roomId;
    public string _roomName;
    public string _hostPlayerName;
    public Int32 _maxPlayerCount;
    public Int32 _playerCount;

    public Room(Int32 roomId, string roomName, Int32 maxPlayerCount, Int32 playerCount, string hostPlayername)
    {
        _roomId = roomId;
        _roomName = roomName;
        _maxPlayerCount = maxPlayerCount;
        _playerCount = playerCount;
        _hostPlayerName = hostPlayername;

    }
}
