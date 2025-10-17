using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LumiaSkill_StormArrow_Copy : PlayerSkill_Copy
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;          // 화살 프리팹
    public Transform firePoint;             // 발사 위치
    public float arrowSpeed = 25f;          // 화살 속도
    public float fireInterval = 0.3f;       // 화살 발사 간격 (3발 = 1초)
    public int arrowCount = 3;              // 발사할 화살 개수

    private bool isFiring = false;

    PhotonView pv;


    private void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        skillName = "폭풍 화살";
        cooldown = 6f;
    }

    public override void Activate()
    {
        if (arrowPrefab == null || firePoint == null || isFiring) return;

        StartCoroutine(FireArrows());
    }

    private IEnumerator FireArrows()
    {
        isFiring = true;

        for (int i = 0; i < arrowCount; i++)
        {
            if (pv != null && pv.IsMine && arrowPrefab != null && firePoint != null)
            {
                // 소유자/팀 식별값 준비
                int ownerTeam = -1;
                var teamA = GetComponentInParent<PlayerTeam>();
                if (teamA != null) ownerTeam = teamA.team;
                int ownerViewID = pv.ViewID;

                object[] data = new object[] { ownerTeam, ownerViewID };

                // InstantiationData 포함해 생성
                var go = PhotonNetwork.Instantiate(
                    arrowPrefab.name,
                    firePoint.position,
                    firePoint.rotation,
                    0,
                    data
                );

                var rb = go.GetComponent<Rigidbody>();
                if (rb != null) rb.velocity = firePoint.forward * arrowSpeed;
            }

            if (i < arrowCount - 1)
                yield return new WaitForSeconds(fireInterval);
        }

        isFiring = false;
    }
}
