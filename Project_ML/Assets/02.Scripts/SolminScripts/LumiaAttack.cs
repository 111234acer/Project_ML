using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaAttack : PlayerAttack
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;         // 화살 프리팹
    public float minArrowSpeed = 10f;      // 최소 속도
    public float maxArrowSpeed = 50f;      // 최대 속도
    public float chargeTime = 1.5f;        // 최대 충전 시간
    private float currentCharge = 0f;

    [Header("References")]
    public Camera playerCamera;            // 플레이어 카메라 (MainCamera)

    private void Update()
    {
        // 공격 버튼 누르고 있으면 충전
        if (Input.GetButton("Fire1"))
        {
            currentCharge += Time.deltaTime;
            currentCharge = Mathf.Min(currentCharge, chargeTime);
            
        }

        // 버튼 떼면 발사
        if (Input.GetButtonUp("Fire1") && CanAttack())
        {
            Attack();
            UpdateFireTime();
        }
    }

    public override void Attack()
    {
        if (arrowPrefab == null || playerCamera == null) return;

        // 발사 방향 = 카메라 중앙
        Vector3 shootDir = playerCamera.transform.forward;

        // 충전 비율 (0 ~ 1)
        float chargePercent = currentCharge / chargeTime;

        // 속도 계산
        float arrowSpeed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, chargePercent);

        // 화살 생성 (카메라 앞에서)
        Vector3 spawnPos = playerCamera.transform.position + shootDir * 0.5f;
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(shootDir));
        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = shootDir * arrowSpeed;
        }

        // 초기화
        currentCharge = 0f;
    }
}