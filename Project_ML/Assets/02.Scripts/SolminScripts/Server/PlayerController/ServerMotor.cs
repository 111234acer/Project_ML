using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class ServerMotor : MonoBehaviourPunCallbacks
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -20f;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float fallMultiplier = 2.5f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Settings")]
    public LayerMask groundMask;

    [Header("Snapshot Settings")]
    [Tooltip("서버가 상태를 방송하는 주기 (Hz)")]
    public float snapshotInterval = 1f / 30f;

    [Header("Performance Settings")]
    [Tooltip("이동 물리 계산만 끄고 싶을 때 true")]
    public bool disableMovement = false; // 이동 연산만 꺼주는 토글

    private CharacterController controller;
    private PlayerHealth_Server health;

    private float lastH, lastV;
    private bool requestJump;
    private float jumpBufferCounter;
    private float velocityY;
    private bool isGrounded;
    private float snapshotTimer;

    private float serverYaw = 0f;

    private AnimationHandler animationHandler;
    private bool prevGrounded;

    private bool ServerActive => PhotonNetwork.IsMasterClient;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth_Server>();
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    private void OnEnable()
    {
        if (!ServerActive)
        {
            enabled = false;
            return;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            enabled = true;
            snapshotTimer = 0f;
        }
        else
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (!ServerActive) return;
        if (health != null && health.isDead) return;

        // 점프 입력 버퍼 처리
        if (requestJump)
        {
            jumpBufferCounter = jumpBufferTime;
            requestJump = false;
        }
        else
        {
            jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!ServerActive) return;
        if (health != null && health.isDead) return;

        // 부하 줄이기: GroundCheck는 이동 시에만 자주 돌림
        if (!disableMovement)
            GroundCheck();

        // 회전값 적용
        transform.rotation = Quaternion.Euler(0f, serverYaw, 0f);

        // 이동 연산 토글
        if (!disableMovement)
        {
            Vector3 move = transform.right * lastH + transform.forward * lastV;
            if (move.sqrMagnitude > 1f) move.Normalize();

            controller.Move(move * moveSpeed * Time.fixedDeltaTime);

            // 점프
            if (isGrounded && jumpBufferCounter > 0f)
            {
                velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
                photonView.RPC("Client_Anim_Jump", RpcTarget.All);
                jumpBufferCounter = 0f;
            }

            // 중력
            if (velocityY < 0f)
                velocityY += gravity * fallMultiplier * Time.fixedDeltaTime;
            else
                velocityY += gravity * Time.fixedDeltaTime;

            controller.Move(Vector3.up * velocityY * Time.fixedDeltaTime);
        }

        // 이동 꺼져 있어도 스냅샷, 회전, 애니메이션은 계속 보냄
        snapshotTimer += Time.fixedDeltaTime;
        if (snapshotTimer >= snapshotInterval)
        {
            snapshotTimer = 0f;
            photonView.RPC("Client_ApplySnapshot", RpcTarget.All,
                transform.position, transform.rotation, velocityY, isGrounded);
        }
    }

    void GroundCheck()
    {
        Vector3 center = controller.bounds.center;
        Vector3 spherePos = new Vector3(center.x, controller.bounds.min.y + 0.05f, center.z);
        float checkRadius = Mathf.Max(controller.radius * 0.9f, 0.2f);

        isGrounded = Physics.CheckSphere(spherePos, checkRadius, groundMask);
        if (isGrounded && velocityY < 0f)
            velocityY = -2f;

        if (!prevGrounded && isGrounded) photonView.RPC("Client_Anim_Land", RpcTarget.All);
        if (prevGrounded && !isGrounded) photonView.RPC("Client_Anim_Fall", RpcTarget.All);
        prevGrounded = isGrounded;
    }

    // ===== 클라이언트 입력 수신 =====
    [PunRPC]
    public void Server_ReceiveInput(int viewID, float h, float v, bool jump, bool dash, float clientTime, PhotonMessageInfo info)
    {
        if (!ServerActive) return;
        if (photonView.ViewID != viewID) return;

        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);
        lastH = h;
        lastV = v;
        if (jump) requestJump = true;

        photonView.RPC("Client_Anim_Move", RpcTarget.All, lastH, lastV);
    }

    // ===== 회전 수신 =====
    [PunRPC]
    public void Server_ReceiveYaw(float yaw)
    {
        if (!ServerActive) return;
        serverYaw = yaw;
    }

    // ===== 애니메이션 브로드캐스트 =====
    [PunRPC] void Client_Anim_Move(float h, float v) => animationHandler?.OnMovement(h, v);
    [PunRPC] void Client_Anim_Jump() => animationHandler?.JumpTrigger();
    [PunRPC] void Client_Anim_Land() => animationHandler?.LandTrigger();
    [PunRPC] void Client_Anim_Fall() => animationHandler?.OnFall();
}
