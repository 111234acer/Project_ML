using System.Collections;
using System.Collections.Generic;
using Photon.Pun.Demo.Cockpit;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;                               // 플레이어 이동속도
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
    public Vector3 velocity;                                   // 현재 속도 (점프/중력 포함)

    [Header("References")]
    public Transform playerCamera;                              // 카메라 transform (PlayerLook에서 참조)
    public PlayerHealth health;                                 // 체력 참조(사망 상태 확인용)

    public PlayerAttack attack;                                 // PlayerAttack에서 attack 참조
    [HideInInspector] public bool isDashing = false;            // 루미아 캐릭터 대시 중인지 확인

    private AnimationHandler animationHandler;
    private bool canJump = true;
    private bool wasGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;               // 마우스 커서 고정
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    void Update()
    {
        // 사망 상태면 입력 완전 차단
        if(health != null && health.isDead)
            return;

        // Look 방향 (카메라 Y 회전)을 플레이어가 따라가도록
        if(playerCamera != null)
        {
            Vector3 lookDir = playerCamera.forward;
            lookDir.y = 0;  // 상하 회전 제거 (수평 방향만)
            if(lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
            }
        }

        // 대시 중일때 입력 이동 중력 무시
        if (!isDashing)            
        {
            GroundCheck();
            HandleJumpInput();
            Move();
        }
    }

    void GroundCheck()
    {
        wasGrounded = isGrounded;

        Vector3 spherePos = new Vector3(controller.bounds.center.x, controller.bounds.min.y + 0.05f, controller.bounds.center.z);
        float checkRadius = Mathf.Max(controller.radius * 0.9f, 0.2f);
        isGrounded = Physics.CheckSphere(spherePos, checkRadius, groundMask);

        if (!wasGrounded && isGrounded)
        {
            animationHandler.LandTrigger();
            if (!Input.GetButton("Jump"))
            {
                canJump = true;
            }
        }
        else if (wasGrounded && !isGrounded)
        {
            animationHandler.OnFall();
            canJump = false;
        }
        else
        {
            if (isGrounded && Input.GetButtonUp("Jump"))
            {
                canJump = true;
            }
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f; // 바닥에 붙도록 살짝 눌러줌
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && canJump)
        {
            jumpBufferCounter = jumpBufferTime;            
        }
        else
            jumpBufferCounter = Mathf.Max(jumpBufferCounter - Time.deltaTime, 0);

        if (isGrounded && jumpBufferCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // 점프 속도 계산
            jumpBufferCounter = 0; // 사용했으니 초기화
            canJump = false;
            animationHandler.JumpTrigger();
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

        animationHandler.OnMovement(horizontal, vertical);
        
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
