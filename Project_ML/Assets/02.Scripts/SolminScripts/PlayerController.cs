using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]                 // CharacterController �ڵ��߰�
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // �÷��̾� �̵��ӵ�
    public float jumpHeight = 2f;                               // ���� ����
    public float gravity = -9.81f;                              // �߷� ��

    private CharacterController controller;
    private Vector3 velocity;                                   // ���� �ӵ� (����/�߷�) ����

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;               // ���콺 Ŀ�� ����(���� �÷����� ���콺 ȭ�� ������ �������� ����
    }
    void Update()
    {
        Move();
    }

    void Move()
    {
        float horzontal = Input.GetAxis("Horizontal");          // A,D
        float vertical = Input.GetAxis("Vertical");             // W,S
        
        Vector3 move = transform.right * horzontal + transform.forward * vertical;
        // �̵� ����
        controller.Move(move * moveSpeed * Time.deltaTime);

        // ���� ��������� �ӵ� �ʱ�ȭ
        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -0.1f;
        }

        // ���� �Է�
        if(Input.GetButton("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // �߷� ����
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
