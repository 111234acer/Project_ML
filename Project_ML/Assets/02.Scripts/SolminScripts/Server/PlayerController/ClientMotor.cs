using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class ClientMotor : MonoBehaviourPun
{
    [Header("Interpolation Settings")]
    [Tooltip("서버 스냅샷을 약간 지연시켜 표시 (초)")]
    public float interpolationDelay = 0.12f;
    [Tooltip("기본 보간 속도 (다른 플레이어용)")]
    public float positionLerpSpeed = 10f;
    [Tooltip("내 캐릭터 보간 가속 비율 (체감 반응 개선용)")]
    public float selfLerpMultiplier = 1.1f;
    [Tooltip("회전 보간 속도")]
    public float rotationLerpSpeed = 7f;

    [Header("Prediction Settings")]
    [Tooltip("이동 속도 (서버와 동일해야 함)")]
    public float moveSpeed = 5f;
    [Tooltip("중력 가속도 (서버와 동일해야 함)")]
    public float gravity = -20f;
    [Tooltip("점프 높이 (서버와 동일해야 함)")]
    public float jumpHeight = 2f;
    [Tooltip("낙하 가속 비율")]
    public float fallMultiplier = 2.5f;
    [Tooltip("서버 위치와 차이날 때 보정 임계값")]
    public float reconciliationThreshold = 0.3f;

    CharacterController controller;
    float velocityY;
    bool isGrounded;

    // 서버 스냅샷 버퍼
    readonly Queue<(float time, Vector3 pos, Quaternion rot)> snapshotBuffer = new();
    Vector3 displayPos;
    Quaternion displayRot;
    float lastT = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        displayPos = transform.position;
        displayRot = transform.rotation;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            PredictLocalMovement();   // 로컬 예측 이동
            ReconcileToServer();      // 서버 스냅샷과 부드러운 보정
        }
    }

    void LateUpdate()
    {
        if (!photonView.IsMine)
            InterpolateSnapshots();   // 다른 플레이어는 보간 처리
    }

    // -----------------------------
    // [1] 로컬 예측 이동
    // -----------------------------
    void PredictLocalMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool jump = Input.GetButton("Jump");

        isGrounded = controller.isGrounded;

        // 이동 방향 계산
        Vector3 move = transform.right * h + transform.forward * v;
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // 입력이 없을 때 이동 완전 정지
        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f)
            move = Vector3.zero;

        controller.Move(move * moveSpeed * Time.deltaTime);

        // 점프
        if (isGrounded && jump)
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // 중력 처리
        if (velocityY < 0f)
            velocityY += gravity * fallMultiplier * Time.deltaTime;
        else
            velocityY += gravity * Time.deltaTime;

        controller.Move(Vector3.up * velocityY * Time.deltaTime);
    }

    // -----------------------------
    // [2] 서버 스냅샷 기반 보정
    // -----------------------------
    void ReconcileToServer()
    {
        if (snapshotBuffer.Count == 0)
            return;

        var latest = snapshotBuffer.Peek();
        float dist = Vector3.Distance(transform.position, latest.pos);

        // 오차가 클 때만 부드럽게 따라감
        if (dist > reconciliationThreshold)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                latest.pos,
                0.05f // 너무 세게 당기면 느려짐 현상 발생, 낮게 유지
            );
        }

        // 회전 보정 (항상 부드럽게)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            latest.rot,
            rotationLerpSpeed * Time.deltaTime
        );
    }

    // -----------------------------
    // [3] 서버 스냅샷 수신
    // -----------------------------
    [PunRPC]
    public void Client_ApplySnapshot(Vector3 pos, Quaternion rot, float velY, bool grounded)
    {
        snapshotBuffer.Enqueue(((float)PhotonNetwork.Time, pos, rot));

        // transform.position 직접 덮어쓰기 금지!
        // 보정은 ReconcileToServer()에서 부드럽게 수행

        while (snapshotBuffer.Count > 10)
            snapshotBuffer.Dequeue();
    }

    // -----------------------------
    // [4] 다른 플레이어 보간 처리
    // -----------------------------
    void InterpolateSnapshots()
    {
        if (snapshotBuffer.Count < 2)
            return;

        float renderTime = (float)PhotonNetwork.Time - interpolationDelay;

        while (snapshotBuffer.Count >= 2 && snapshotBuffer.Peek().time <= renderTime)
            snapshotBuffer.Dequeue();

        var array = snapshotBuffer.ToArray();
        if (array.Length < 2)
            return;

        var older = array[0];
        var newer = array[1];

        float total = newer.time - older.time;
        if (total <= 0.001f) total = 0.001f;

        float elapsed = renderTime - older.time;
        float t = Mathf.Clamp01(elapsed / total);
        t = Mathf.Lerp(lastT, t, 0.6f);
        lastT = t;

        displayPos = Vector3.Lerp(older.pos, newer.pos, t);
        displayRot = Quaternion.Slerp(older.rot, newer.rot, t);

        transform.position = Vector3.Lerp(
            transform.position,
            displayPos,
            positionLerpSpeed * Time.deltaTime
        );
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            displayRot,
            rotationLerpSpeed * Time.deltaTime
        );
    }
}
