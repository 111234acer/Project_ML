using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class ClientMotor : MonoBehaviourPun
{
    [Header("Lerp Settings")]
    [Tooltip("기본 보간 속도 (다른 플레이어용)")]
    public float positionLerpSpeed = 10f;

    [Tooltip("내 캐릭터 보간 가속 비율 (체감 반응 개선용)")]
    public float selfLerpMultiplier = 3f;

    [Tooltip("회전 보간 속도")]
    public float rotationLerpSpeed = 10f;

    // 서버에서 받은 목표 위치/회전
    private Vector3 targetPos;
    private Quaternion targetRot;

    private void Start()
    {
        targetPos = transform.position;
        targetRot = transform.rotation;
    }

    private void Update()
    {
        // 모든 캐릭터는 서버의 스냅샷을 따라간다.
        // 단, 내 캐릭터는 조금 더 빠르게 보간하여 입력 지연을 줄인다.
        if (photonView.IsMine)
            SmoothMoveSelf();
        else
            SmoothMoveOthers();
    }

    // 내 캐릭터: 서버 위치를 빠르게 따라감 (조작감 향상)
    void SmoothMoveSelf()
    {
        float lerpFactor = positionLerpSpeed * selfLerpMultiplier * Time.deltaTime;

        transform.position = Vector3.Lerp(transform.position, targetPos, lerpFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * selfLerpMultiplier * Time.deltaTime);
    }

    // 다른 캐릭터: 자연스러운 보간 이동
    void SmoothMoveOthers()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);
    }

    // 서버에서 보내는 스냅샷 수신 (ServerMotor → ClientMotor)
    [PunRPC]
    public void Client_ApplySnapshot(Vector3 pos, Quaternion rot, float velY, bool grounded)
    {
        // Debug.Log($"[ClientMotor] Snapshot: {pos}");
        targetPos = pos;
        targetRot = rot;
    }
}
