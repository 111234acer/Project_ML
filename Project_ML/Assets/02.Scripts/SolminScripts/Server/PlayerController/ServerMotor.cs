using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class ServerMotor : MonoBehaviourPun
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;            // 이동 속도
    public float gravity = -20f;            // 중력 값

    [Header("Jump Settings")]
    public float jumpHeight = 2f;           // 점프 높이
    public float fallMultiplier = 2.5f;     // 하강 중 중력 배수
    public float jumpBufferTime = 0.1f;     // 점프 입력 버퍼 시간

    [Header("Ground Settings")]
    public LayerMask groundMask;            // 지면 레이어

    [Header("Snapshot Settings")]
    [Tooltip("서버가 상태를 방송하는 주기 (1/15 = 15Hz 권장)")]
    public float snapshotInterval = 1f / 15f;

    // 내부 구성요소
    private CharacterController controller;
    private PlayerHealth_Server health;

    // 내부 상태
    private float lastH, lastV;
    private bool requestJump;
    private float jumpBufferCounter;
    private float velocityY;
    private bool isGrounded;
    private float snapshotTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth_Server>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return; // 서버만 실행
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
        if (!PhotonNetwork.IsMasterClient) return; // 서버만 실행
        if (health != null && health.isDead) return;

        GroundCheck();

        // 이동 방향 계산
        Vector3 move = transform.right * lastH + transform.forward * lastV;
        if (move.sqrMagnitude > 1f) move.Normalize();

        // 이동
        controller.Move(move * moveSpeed * Time.fixedDeltaTime);

        // 점프 처리
        if (isGrounded && jumpBufferCounter > 0f)
        {
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
        }

        // 중력 처리
        if (velocityY < 0f)
            velocityY += gravity * fallMultiplier * Time.fixedDeltaTime;
        else
            velocityY += gravity * Time.fixedDeltaTime;

        controller.Move(new Vector3(0f, velocityY, 0f) * Time.fixedDeltaTime);

        // 상태 스냅샷 전송 (15Hz)
        snapshotTimer += Time.fixedDeltaTime;
        if (snapshotTimer >= snapshotInterval)
        {
            snapshotTimer = 0f;
            photonView.RPC("Client_ApplySnapshot", RpcTarget.All,
                transform.position, transform.rotation, velocityY, isGrounded);
            //Debug.Log($"[ServerMotor] Snapshot broadcast pos:{transform.position}"); 멀티테스트 오류 확인용
        }
    }

    void GroundCheck()
    {
        Vector3 center = controller.bounds.center;
        Vector3 spherePos = new Vector3(center.x, controller.bounds.min.y + 0.05f, center.z);
        float checkRadius = Mathf.Max(controller.radius * 0.9f, 0.2f);

        isGrounded = Physics.CheckSphere(spherePos, checkRadius, groundMask);
        if (isGrounded && velocityY < 0f)
            velocityY = -2f; // 바닥에 붙이기
    }

    // ===== 클라이언트 입력 수신 (서버만 호출) =====
    [PunRPC]
    public void Server_ReceiveInput(int viewID, float h, float v, bool jump, bool dash, float clientTime, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (photonView.ViewID != viewID) return; // 내 플레이어만

        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);

        lastH = h;
        lastV = v;
        if (jump) requestJump = true;

        //Debug.Log($"[ServerMotor] Receive input from {info.Sender.ActorNumber} h:{h} v:{v}"); 멀티플레이 오류 확인용
    }

    // 서버가 마우스 회전 수신
    [PunRPC]
    public void Server_ReceiveLook(float mouseX,PhotonMessageInfo info)
    {
        if(!PhotonNetwork.IsMasterClient) return;

        // Y축 회전 적용(몸체만)
        transform.Rotate(Vector3.up * mouseX);
    }
}