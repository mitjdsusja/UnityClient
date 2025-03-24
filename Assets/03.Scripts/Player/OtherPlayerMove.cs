using System;
using TMPro;
using UnityEngine;

public class OtherPlayerMove : MonoBehaviour
{
    Rigidbody _rigidbody;
    string _playerInfoKey;

    public float _jumpHeight = 2f;
    private bool _isGrounded = false;

    //
    Vector3 _lastReceivedPosition;
    private float _interpolationTime = 100f;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _lastReceivedPosition = transform.position;
    }

    void Update()
    {

    }

    public void SetPlayerInfoKey(string key)
    {
        _playerInfoKey = key;
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        PlayerInfo playerInfo = GameManager.Instance.GetPlayerInfo(_playerInfoKey);

        Vector3 lastPosition = playerInfo.GetPosition();
        Vector3 lastVelocity = playerInfo.GetVelocity();
        long lastTimestamp = playerInfo.GetLastMovePacketTime();

        long curTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float timeElapsed = (curTimestamp - lastTimestamp) / 1000.0f; // 밀리초 → 초 변환


    }

    void Jump()
    {
        
    }
}
