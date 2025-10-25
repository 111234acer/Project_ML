using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class ServerMotor : MonoBehaviourPun
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
    [Tooltip("서버가 상태를 방송하는 주기")]
    public float snapshotInterval = 1f / 15f;

    private CharacterController controller;
    private PlayerHealth_Server health;

    // 내부 상태
    private float lastH, lastV;
    private bool requestJump;
    private float jumpBufferCounter;
    private float velocityY;
    private bool isGrounded;
    private float snapshotTimer;

    // 서버가 유지하는 회전값
    private float serverYaw = 0f;

    private AnimationHandler animationHandler;
    private bool prevGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth_Server>();
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (health != null && health.isDead) return;

        // 점프 입력 버퍼
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
        if (!PhotonNetwork.IsMasterClient) return;
        if (health != null && health.isDead) return;

        GroundCheck();

        // 서버 회전값 적용 (yaw 기준)
        transform.rotation = Quaternion.Euler(0f, serverYaw, 0f);

        // 이동 방향 계산
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

        // 중력 처리
        if (velocityY < 0f)
            velocityY += gravity * fallMultiplier * Time.fixedDeltaTime;
        else
            velocityY += gravity * Time.fixedDeltaTime;

        controller.Move(Vector3.up * velocityY * Time.fixedDeltaTime);

        // 스냅샷 주기 전송 (15Hz)
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
        if (!PhotonNetwork.IsMasterClient) return;
        if (photonView.ViewID != viewID) return;

        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);
        lastH = h;
        lastV = v;
        if (jump) requestJump = true;

        photonView.RPC("Client_Anim_Move", RpcTarget.All, lastH, lastV);
    }

    // ===== 클라이언트에서 회전값 수신 (절대 yaw) =====
    [PunRPC]
    public void Server_ReceiveYaw(float yaw)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        serverYaw = yaw;
    }

    [PunRPC] 
    void Client_Anim_Move(float h, float v) 
    { 
        animationHandler?.OnMovement(h, v);
    }
    [PunRPC]
    void Client_Anim_Jump() 
    { 
        animationHandler?.JumpTrigger();
    }
    [PunRPC] 
    void Client_Anim_Land() 
    { 
        animationHandler?.LandTrigger();
    }
    [PunRPC]
    void Client_Anim_Fall()
    {
        animationHandler?.OnFall();
    }
}
