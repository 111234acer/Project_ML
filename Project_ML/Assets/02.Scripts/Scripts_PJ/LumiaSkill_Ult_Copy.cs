using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LumiaSkill_Ult_Copy : PlayerSkill_Copy
{
    [Header("AOE Settings")]
    public GameObject indicatorPrefab;
    public GameObject aoeEffectPrefab;
    public float range = 20f;
    public float aoeRadius = 5f;            // ← 네트워크로 넘길 반지름

    [Header("Damage Settings")]
    public float duration = 5f;
    public float damagePerSecond = 30f;

    [Header("References")]
    public Camera playerCamera;
    public Transform player;

    private GameObject indicatorInstance;
    private bool isTargeting = false;

    PlayerSkillManager_Copy skillMgr;
    PhotonView pv;

    private void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        skillMgr = GetComponentInParent<PlayerSkillManager_Copy>();
        skillName = "";
        cooldown = 15f;
    }

    public override void Activate()
    {
        // 원격 클라는 입력/조준 안 함(쿨타임 동기화만 받음)
        if (PhotonNetwork.InRoom && pv != null && !pv.IsMine) return;
        if (isTargeting) return;

        StartCoroutine(TargetingRoutine());
    }

    private IEnumerator TargetingRoutine()
    {
        isTargeting = true;

        // 로컬 전용 인디케이터
        indicatorInstance = Instantiate(indicatorPrefab);
        var ind = indicatorInstance.GetComponent<AOEIndicator>();
        if (ind != null) ind.radius = aoeRadius;

        while (isTargeting)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                float distance = Vector3.Distance(player.position, hit.point);
                if (distance <= range)
                    indicatorInstance.transform.position = hit.point;
            }

            // 좌클릭: 시전 확정 -> 네트워크 생성
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 spawnPos = indicatorInstance.transform.position;                

                int ownerTeam = -1;
                var team = GetComponentInParent<PlayerTeam>();
                if (team != null) ownerTeam = team.team;
                int ownerViewID = (pv != null) ? pv.ViewID : -1;

                object[] data = new object[]
                {
                    ownerTeam,
                    ownerViewID,
                    aoeRadius,
                    damagePerSecond,
                    duration
                };

                PhotonNetwork.Instantiate(aoeEffectPrefab.name, spawnPos, Quaternion.identity, 0, data);

                if (pv && pv.IsMine)
                    pv.RPC("RPC_UseSkill", RpcTarget.All, 3, Time.time);

                if (skillMgr != null) skillMgr.suppressFire1Until = Time.time + 0.5f;

                Destroy(indicatorInstance);
                isTargeting = false;
                break;
            }

            // ESC로 취소
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
