using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LumiaSkill_StormArrow : PlayerSkill
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;          // 화살 프리팹
    public Transform firePoint;             // 발사 위치
    public float arrowSpeed = 25f;          // 화살 속도
    public float fireInterval = 0.3f;       // 화살 발사 간격 (3발 = 1초)
    public int arrowCount = 3;              // 발사할 화살 개수

    private bool isFiring = false;

    private void Awake()
    {
        skillName = "폭풍 화살";
        cooldown = 6f;
    }

    public override void Activate()
    {
        if(arrowPrefab == null || firePoint == null || isFiring) return;

        StartCoroutine(FireArrows());
    }

    private IEnumerator FireArrows()
    {
        isFiring = true;

        for(int i = 0; i < arrowCount; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();

            if(rb != null)
            {
                rb.velocity = firePoint.forward * arrowSpeed;

                yield return new WaitForSeconds(fireInterval);
            }

            isFiring = false;
        }
    }
}
