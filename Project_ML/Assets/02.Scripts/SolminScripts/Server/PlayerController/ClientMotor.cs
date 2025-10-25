using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class ClientMotor : MonoBehaviourPun
{
    [Header("Interpolation Settings")]
    [Tooltip("서버 스냅샷을 약간 지연시켜 표시 (초)")]
    public float interpolationDelay = 0.08f;
    [Tooltip("기본 보간 속도 (다른 플레이어용)")]
    public float positionLerpSpeed = 10f;
    [Tooltip("내 캐릭터 보간 가속 비율 (체감 반응 개선용)")]
    public float selfLerpMultiplier = 1.1f;
    [Tooltip("회전 보간 속도")]
    public float rotationLerpSpeed = 7f;

    // [PREDICTION] 추가: 이동 관련 변수
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
    public float reconciliationThreshold = 0.25f;

    CharacterController controller;

    // 예측용 변수
    private float velocityY;
    private bool isGrounded;

    // 서버에서 받은 스냅샷 버퍼
    private readonly Queue<(float time, Vector3 pos, Quaternion rot)> snapshotBuffer = new();
    private Vector3 displayPos;
    private Quaternion displayRot;
    private float lastT = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        displayPos = transform.position;
        displayRot = transform.rotation;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            // [PREDICTION] 로컬 이동 예측 실행
            PredictLocalMovement();
        }
    }

    private void LateUpdate()
    {
        // [INTERPOLATION] 다른 플레이어만 보간
        if (!photonView.IsMine)
            InterpolateSnapshots();
    }

    // [PREDICTION] 로컬 이동 계산 (서버 이동 수식 복제)

    void PredictLocalMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool jump = Input.GetButton("Jump");

        isGrounded = controller.isGrounded;

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // 이동
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 점프
        if (isGrounded && jump)
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // 중력
        if (velocityY < 0f)
            velocityY += gravity * fallMultiplier * Time.deltaTime;
        else
            velocityY += gravity * Time.deltaTime;

        controller.Move(Vector3.up * velocityY * Time.deltaTime);
    }

    // [RECONCILIATION] 서버 위치 수신 시 오차 보정
    [PunRPC]
    public void Client_ApplySnapshot(Vector3 pos, Quaternion rot, float velY, bool grounded)
    {
        snapshotBuffer.Enqueue(((float)PhotonNetwork.Time, pos, rot));

        // [RECONCILIATION] 내 캐릭터의 서버 보정 처리
        if (photonView.IsMine)
        {
            float dist = Vector3.Distance(transform.position, pos);
            if (dist > reconciliationThreshold)
            {
                // 서버 위치와 클라 위치가 다르면 보정
                transform.position = Vector3.Lerp(transform.position, pos, 0.5f);
            }
        }

        // 메모리 누적 방지
        while (snapshotBuffer.Count > 10)
            snapshotBuffer.Dequeue();
    }

    // [INTERPOLATION] 다른 플레이어 부드럽게 표시
    private void InterpolateSnapshots()
    {
        if (snapshotBuffer.Count < 2)
            return;

        float renderTime = (float)PhotonNetwork.Time - interpolationDelay;

        while (snapshotBuffer.Count >= 2 && snapshotBuffer.Peek().time <= renderTime)
            snapshotBuffer.Dequeue();

        var array = snapshotBuffer.ToArray();
        if (array.Length < 2) return;

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

        transform.position = Vector3.Lerp(transform.position, displayPos, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, displayRot, rotationLerpSpeed * Time.deltaTime);
    }
}
