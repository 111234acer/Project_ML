using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // 플레이어 이동속도
    public float gravity = -20f;                                 // 중력 값

    [Header("Jump Settings")]
    public float jumpHeight = 2f;                               // 점프 높이
    public float fallMultiplier = 2.5f;                         // 하강 시 중력 배수
    public float jumpBufferTime = 0.1f;                         // 점프 입력 버퍼 시간
    private float jumpBufferCounter;                            // 내부 카운터

    [Header("Ground Settings")]
    public LayerMask groundMask;                                // 바닥 레이어
    private bool isGrounded;                                    // 직접 체크한 바닥 여부 

    private CharacterController controller;
    private Vector3 velocity;                                   // 현재 속도 (점프/중력 포함)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;               // 마우스 커서 고정
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
            velocity.y = -2f; // 바닥에 붙도록 살짝 눌러줌
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter = Mathf.Max(jumpBufferCounter - Time.deltaTime, 0);

        if (isGrounded && jumpBufferCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // 점프 속도 계산
            jumpBufferCounter = 0; // 사용했으니 초기화
        }
    }

    void ApplyGravity()
    {
        // if문으로 바꾸어 FPS 느낌 점프/낙하 구현
        if (velocity.y < 0) // 떨어지는 중
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else // 올라가는 중
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
