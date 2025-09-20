using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // �÷��̾� �̵��ӵ�
    public float gravity = -20f;                                 // �߷� ��

    [Header("Jump Settings")]
    public float jumpHeight = 2f;                               // ���� ����
    public float fallMultiplier = 2.5f;                         // �ϰ� �� �߷� ���
    public float jumpBufferTime = 0.1f;                         // ���� �Է� ���� �ð�
    private float jumpBufferCounter;                            // ���� ī����

    [Header("Ground Settings")]
    public LayerMask groundMask;                                // �ٴ� ���̾�
    private bool isGrounded;                                    // ���� üũ�� �ٴ� ���� 

    private CharacterController controller;
    private Vector3 velocity;                                   // ���� �ӵ� (����/�߷� ����)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;               // ���콺 Ŀ�� ����
    }

    void Update()
    {
        GroundCheck();
        HandleJumpInput();
        Move();
    }

    void GroundCheck()
    {
        Vector3 spherePos = new Vector3(controller.bounds.center.x,
                                        controller.bounds.min.y + 0.05f,
                                        controller.bounds.center.z);
        float checkRadius = Mathf.Max(controller.radius * 0.9f, 0.2f);
        isGrounded = Physics.CheckSphere(spherePos, checkRadius, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // �ٴڿ� �ٵ��� ��¦ ������
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter = Mathf.Max(jumpBufferCounter - Time.deltaTime, 0);

        if (isGrounded && jumpBufferCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // ���� �ӵ� ���
            jumpBufferCounter = 0; // ��������� �ʱ�ȭ
        }
    }

    void ApplyGravity()
    {
        // if������ �ٲپ� FPS ���� ����/���� ����
        if (velocity.y < 0) // �������� ��
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else // �ö󰡴� ��
            velocity.y += gravity * Time.deltaTime;
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (controller == null) return;

        Gizmos.color = Color.green;
        Vector3 spherePos = new Vector3(controller.bounds.center.x,
                                        controller.bounds.min.y + 0.05f,
                                        controller.bounds.center.z);
        float checkRadius = Mathf.Max(controller.radius * 0.9f, 0.2f);
        Gizmos.DrawWireSphere(spherePos, checkRadius);
    }
}
