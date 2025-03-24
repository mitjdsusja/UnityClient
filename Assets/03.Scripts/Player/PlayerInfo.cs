using System;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct Position
{
    public float x;
    public float y;
    public float z;
}
[System.Serializable]
public struct Velocity {
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class PlayerInfo
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private Int32 _level;

    [SerializeField]
    private UInt64 _userId = 0;
    [SerializeField]
    private Vector3 _position = new Vector3();
    [SerializeField]
    private Vector3 _velocity = new Vector3();

    long _lastMovePacketTime = 0;

    public PlayerInfo()
    {

    }

    // Name
    public string GetName()
    {
        return _name;
    }
    public void SetName(string name) {
        _name = name; 
    }

    // Level
    public Int32 GetLevel()
    {
        return _level;
    }
    public void SetLevel(Int32 level)
    {
        _level = level;
    }

    // User ID
    public UInt64 GetUserId()
    {
        return _userId;
    }
    public void SetUserId(UInt64 userId)
    {
        _userId = userId;
    }

    // Position
    public Vector3 GetPosition()
    {
        return this._position;
    }
    public void SetPosition(Vector3 position)
    {
        _position = position;
    }
    public void SetPosition(float x, float y, float z)
    {
        _position.x = x;
        _position.y = y;
        _position.z = z;
    }

    // Velocity
    public Vector3 GetVelocity()
    {
        return this._velocity;
    }
    public void SetVelocity(Vector3 velocity)
    {
        _velocity = velocity;
    }
    public void SetVelocity(float x, float y, float z)
    {
        _velocity.x = x;
        _velocity.y = y;    
        _velocity.z = z;
    }

    // Last Update Time
    public long GetLastMovePacketTime()
    {
        return _lastMovePacketTime;
    }
    public void SetLastMovePacketTime(long time)
    {
        _lastMovePacketTime = time;
    }
}
