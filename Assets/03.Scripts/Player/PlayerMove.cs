using MsgTest;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 10f;  
    public float jumpHeight = 2f; 

    private Vector3 dir;
    private bool isGrounded = false;

    Rigidbody _rigidbody;

    //
    private float sendInterval = 0.1f;
    private float lastSendTime = 0f;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        dir = Vector3.zero;
    }

    void Update()
    {
        dir.x = Input.GetAxisRaw("Horizontal");
        dir.z = Input.GetAxisRaw("Vertical");
        dir.Normalize();

        GameManager.Instance._playerInfo.SetPosition(transform.position);
        Jump();

        if (Time.time - lastSendTime >= sendInterval && GameManager.Instance._isRoomEnter)
        {
            Vector3 currentPosition = transform.position;
            Vector3 currentVelocity = _rigidbody.linearVelocity;

            PacketSender.Instance.SendPlayerPosition(currentPosition, dir * moveSpeed);

            lastSendTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        _rigidbody.MovePosition(transform.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded) // "Jump"�� Unity���� �⺻������ "Space" Ű�� ���ε�
        {
            Vector3 jumpVelocity = Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z); // ���� ���� �ӵ��� �ʱ�ȭ
            _rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange); // ���� �� �߰�
            isGrounded = false; // ���� ���̹Ƿ� isGrounded�� false�� ����
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; 
        }
    }
    void SendPositionToServer()
    {
        
    }
}
