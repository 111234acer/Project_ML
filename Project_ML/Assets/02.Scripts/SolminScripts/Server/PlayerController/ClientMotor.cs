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

    // 서버 스냅샷 버퍼
    readonly Queue<(float time, Vector3 pos, Quaternion rot)> snapshotBuffer = new();
    Vector3 displayPos;
    Quaternion displayRot;
    float lastT = 0f;

    void Start()
    {
        displayPos = transform.position;
        displayRot = transform.rotation;
    }

    void LateUpdate()
    {
        if (!photonView.IsMine)
            InterpolateSnapshots();   // 다른 플레이어는 보간 처리
    }

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

    [PunRPC]
    public void Client_ApplySnapshot(Vector3 pos, Quaternion rot, float velY, bool grounded)
    {
        snapshotBuffer.Enqueue(((float)PhotonNetwork.Time, pos, rot));

        while (snapshotBuffer.Count > 10)
            snapshotBuffer.Dequeue();

        Debug.Log($"[ClientMotor] Received snapshot for ViewID {photonView.ViewID}");
    }
}
