using UnityEngine;
using Photon.Pun;
using System;

public class LumiaAttack_Net : PlayerAttack_Net
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;         // 화살 프리팹
    public float minArrowSpeed = 10f;      // 최소 속도
    public float maxArrowSpeed = 50f;      // 최대 속도
    public float chargeTime = 1f;          // 최대 충전 시간
    private float currentCharge = 0f;

    [Header("References")]
    public Camera playerCamera;

    private bool isCharging = false;

    public static Action<float, bool> OnChargeUpdate;   // UI 갱신용 이벤트

    private AnimationHandler animationHandler;


    void Awake()
    {
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 스킬 사용 중이면 공격 차단
        if (PlayerSkillManager_Net.IsUsingAnySkill)
            return;

        // 마우스 좌클릭 -> 차징 시작
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentCharge = 0f;
            OnChargeUpdate?.Invoke(0f, true);
            animationHandler?.OnAim();
        }

        // 유지 중 : 차징 진행
        if (isCharging && Input.GetMouseButton(0))
        {
            currentCharge = Mathf.Min(currentCharge + Time.deltaTime, chargeTime);
            float percent = Mathf.Clamp01(currentCharge / chargeTime);
            OnChargeUpdate?.Invoke(percent, true);
        }

        // 버튼 때면 공격 요청
        if (isCharging && Input.GetMouseButtonUp(0))
        {
            float sendCharge = currentCharge; // chargeTime 기준 원본 값 전달
            isCharging = false;
            OnChargeUpdate?.Invoke(0f, false);
            RequestAttack(sendCharge); // 서버 권위 경로로 발사 요청
        }
    }

    protected override void Attack(float charge)
    {
        float percent = Mathf.Clamp01(charge / chargeTime);
        float speed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, percent);

        Vector3 dir = playerCamera.transform.forward;
        Vector3 spawnPos = firePoint.position;

        // 서버가 화살 생성
        GameObject arrow = PhotonNetwork.Instantiate(arrowPrefab.name, spawnPos, Quaternion.LookRotation(dir));
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = dir * speed;
    }

    [PunRPC]
    protected override void Client_OnAttack(float charge)
    {
        animationHandler?.ShootTrigger();
    }
}
