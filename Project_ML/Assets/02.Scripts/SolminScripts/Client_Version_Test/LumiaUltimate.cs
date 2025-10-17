using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaUltimate : PlayerSkill
{
    [Header("AOE Settings")]
    public GameObject indicatorPrefab;      // 조준 범위 표시용 프리팹
    public GameObject aoeEffectPrefab;      // 실제 공격 이펙트
    public float range = 20f;               // 시전 기능 처리

    [Header("Damage Settings")]
    public float duration = 5f;             // AOE 지속시간
    public float damagePerSecond = 30f;     // 초당 피해

    [Header("References")]
    public Camera playerCamera;             // 플레이어 카메라
    public Transform player;                // 플레이어 본체 위치

    private GameObject indicatorInstance;   // 현재 활성화된 인디케이터
    private bool isTargeting = false;       // 조준 상태 여부
    
    private void Awake()
    {
        skillName = "";
        cooldown = 15f;
    }

    public override void Activate()
    {
        if (isTargeting) return;
        StartCoroutine(TargetingRoutine());
    }

    private IEnumerator TargetingRoutine()
    {
        isTargeting = true;

        // 조준용 인디케이터 생성
        indicatorInstance = Instantiate(indicatorPrefab);

        while (isTargeting)
        {
            // 마우스 포인터 기준 지면 위치 찾기
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray,out RaycastHit hit, 100f))
            {
                 // 플레이어 기준 range내에서만 인디케이터 이동
                 float distance = Vector3.Distance(player.position,hit.point);
                if(distance <= range)
                    indicatorInstance.transform.position = hit.point;
            }

            // 좌클릭 이동
            if (Input.GetMouseButtonDown(0))
            {
                // 마우스 위치 무시하고 인디케이터 중심에 발동
                Vector3 spawnPos = indicatorInstance.transform.position;
                Quaternion spawnRot = Quaternion.identity;

                GameObject aoe = Instantiate(aoeEffectPrefab,spawnPos, spawnRot);
                aoe.GetComponent<AOEDamageZone>()?.Initialize(damagePerSecond,duration);

                Destroy(indicatorInstance);
                isTargeting = false;
                break;
            }

            // 우클릭으로 취소
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(indicatorInstance);
                isTargeting = false;
                break;
            }

            yield return null;
        }
    }
}
