using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class ClientMotor : MonoBehaviourPun
{
    [Header("Interpolation Settings")]
    [Tooltip("서버 스냅샷을 약간 지연시켜 표시 (초)")]
    public float interpolationDelay = 0.12f; // 100ms 정도 지연

    [Tooltip("기본 보간 속도 (다른 플레이어용)")]
    public float positionLerpSpeed = 10f;

    [Tooltip("내 캐릭터 보간 가속 비율 (체감 반응 개선용)")]
    public float selfLerpMultiplier = 1.1f;

    [Tooltip("회전 보간 속도")]
    public float rotationLerpSpeed = 7f;

    // 서버에서 받은 스냅샷 버퍼
    private readonly Queue<(float time, Vector3 pos, Quaternion rot)> snapshotBuffer = new();

    // 보간용 현재 표시 위치/회전
    private Vector3 displayPos;
    private Quaternion displayRot;

    private float lastT = 0f;

    private void Start()
    {
        displayPos = transform.position;
        displayRot = transform.rotation;
    }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        InterpolateSnapshots();
    }

    private void InterpolateSnapshots()
    {
        if (snapshotBuffer.Count < 2)
            return; // 최소 2개 이상 쌓여야 보간 가능

        // 랜더 기준 시간(약간 과거 시점)
        float renderTime = (float)PhotonNetwork.Time - interpolationDelay;

        // 오래된 스냅샷은 버림
        while (snapshotBuffer.Count >= 2 && snapshotBuffer.Peek().time <= renderTime)
            snapshotBuffer.Dequeue();

        var array = snapshotBuffer.ToArray();
        if (array.Length < 2)
            return;

        // 앞뒤 스냅샷 사이의 보간 비율 계산
        var older = array[0];
        var newer = array[1];

        // **보간 비율 계산 안정화**
        float total = newer.time - older.time;
        if (total <= 0.001f) total = 0.001f; // 0 나눗셈 방지

        float elapsed = renderTime - older.time;
        float t = Mathf.Clamp01(elapsed / total);

        t = Mathf.Lerp(lastT, t, 0.6f);
        lastT = t;

        displayPos = Vector3.Lerp(older.pos, newer.pos, t);
        displayRot = Quaternion.Slerp(older.rot, newer.rot, t);

        // 내 캐릭터/다른 캐릭터에 따라 보간 속도 다르게 적용
        if (photonView.IsMine)
            SmoothMoveSelf();
        else
            SmoothMoveOthers();
    }

    // 내 캐릭터: 서버 위치를 빠르게 따라감 (체감 반응 개선)
    void SmoothMoveSelf()
    {
        float lerpFactor = positionLerpSpeed * selfLerpMultiplier * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, displayPos, lerpFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation, displayRot, rotationLerpSpeed * selfLerpMultiplier * Time.deltaTime);
    }


    // 다른 캐릭터: 자연스러운 보간 이동
    void SmoothMoveOthers()
    {
        transform.position = Vector3.Lerp(transform.position, displayPos, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, displayRot, rotationLerpSpeed * Time.deltaTime);
    }


    // 서버에서 스냅샷 수신 (ServerMotor → ClientMotor)
    [PunRPC]
    public void Client_ApplySnapshot(Vector3 pos, Quaternion rot, float velY, bool grounded)
    {
        snapshotBuffer.Enqueue(((float)PhotonNetwork.Time, pos, rot));

        // 메모리 누적 방지    
        while (snapshotBuffer.Count > 10)
            snapshotBuffer.Dequeue();
    }
}
