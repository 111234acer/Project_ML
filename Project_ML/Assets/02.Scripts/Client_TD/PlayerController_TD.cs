using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController_TD : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("플레이어 이동 속도")]
    public float moveSpeed = 5.0f;

    [Header("점프 설정")]
    [Tooltip("점프 높이")]
    public float jumpHeight = 2f;
    [Tooltip("기본 중력 값 (음수 권장)")]
    public float gravity = -20f;
    [Tooltip("하강 시 추가 중력 배수 (빠르게 떨어짐)")]
    public float fallMultiplier = 2.5f;
    [Tooltip("점프 입력 버퍼 시간 (착지 직전 점프 입력 유예)")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    [Header("지면 감지")]
    [Tooltip("바닥 감지를 위한 레이어")]
    public LayerMask groundMask;
    [Tooltip("바닥 감지 여유 높이")]
    public float groundYOffset = 0.05f;
    [Tooltip("바닥 감지 구체 반경 (0이면 자동 계산)")]
    public float groundCheckRadius = 0f;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("참조")]
    [Tooltip("플레이어 카메라 Transform (시야 방향 기준)")]
    public Transform playerCamera;
    [Tooltip("타워 매니저 (HP 0 시 게임 종료)")]
    private TowerManager_TD towerManager;
    [Tooltip("게임 매니저 (카드 선택 / 일시정지 상태 확인용)")]
    private SingleGameManager_TD singleGameManager;
    [Tooltip("기본 공격 스크립트 (자동 공격용)")]
    private PlayerAttack_TD playerAttack;
    [Tooltip("애니메이션 제어 핸들러 (이동 / 점프 / 착지 등)")]
    private AnimationHandler animationHandler;

    // 내부 전용
    private CharacterController controller;
    private Vector3 velocity;   // 수직 속도 (점프/낙하)
    private bool canJump = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (!animationHandler)
            animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 게임 상태 확인 (입력 차단 조건)
        if (towerManager && towerManager.isDestroyed) return;       // 타워 파괴 시 정지
        if (singleGameManager && singleGameManager.isPaused) return; // 일시정지 / 카드 선택 시 정지

        GroundCheck();
        HandleJumpInput();
        Move();

        // 공격은 PlayerAttack_TD에서 자동으로 수행됨
    }

    // 지면 감지 처리
    void GroundCheck()
    {
        wasGrounded = isGrounded;

        Vector3 spherePos = new Vector3(
            controller.bounds.center.x,
            controller.bounds.min.y + groundYOffset,
            controller.bounds.center.z
        );

        float radius = groundCheckRadius > 0f
            ? groundCheckRadius
            : Mathf.Max(controller.radius * 0.9f, 0.2f);

        isGrounded = Physics.CheckSphere(spherePos, radius, groundMask);

        if (!wasGrounded && isGrounded)
        {
            if (animationHandler) animationHandler.LandTrigger();
            if (!Input.GetButton("Jump"))
                canJump = true;
        }
        else if (wasGrounded && !isGrounded)
        {
            if (animationHandler) animationHandler.OnFall();
            canJump = false;
        }
        else
        {
            if (isGrounded && Input.GetButtonUp("Jump"))
                canJump = true;

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f; // 바닥에 붙이기
        }
    }

    // 점프 입력 처리
    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && canJump)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter = Mathf.Max(jumpBufferCounter - Time.deltaTime, 0);
        }

        if (isGrounded && jumpBufferCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0;
            canJump = false;

            if (animationHandler)
                animationHandler.JumpTrigger();
        }
    }


    // 중력 적용
    void ApplyGravity()
    {
        if (velocity.y < 0)
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else
            velocity.y += gravity * Time.deltaTime;
    }

    // 이동 처리
    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (animationHandler)
            animationHandler.OnMovement(horizontal, vertical);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);
    }

    // 바닥 감지 Gizmo (디버그용)
    private void OnDrawGizmosSelected()
    {
        if (!controller) controller = GetComponent<CharacterController>();
        if (!controller) return;

        Gizmos.color = Color.green;

        Vector3 spherePos = new Vector3(
            controller.bounds.center.x,
            controller.bounds.min.y + groundYOffset,
            controller.bounds.center.z
        );

        float radius = groundCheckRadius > 0f
            ? groundCheckRadius
            : Mathf.Max(controller.radius * 0.9f, 0.2f);

        Gizmos.DrawWireSphere(spherePos, radius);
    }
}
