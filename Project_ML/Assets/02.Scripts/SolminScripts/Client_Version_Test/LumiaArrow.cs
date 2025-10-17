using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaArrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    public int damage = 30;                     // 공격력
    public float lifeTime = 5f;                 // 자동 제거 시간
    public string targetTag = "RedPlayer";      // 맞을 대상(적을 RedPlayer로 임시 설정)

    private void Start()
    {
        // 일정 시간 지나면 제거
        Destroy(gameObject,lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 타겟이 player가 아니면 벽이나 지형이므로 제거
        if (!collision.collider.CompareTag(targetTag))
        {
            Destroy(gameObject);
            return;
        }

        // PlayerHealth 탐색(HeadCollider 대비)
        PlayerHealth target = collision.collider.GetComponentInParent<PlayerHealth>();
        if (target == null)
        {
            return;
        }

        int finalDamage = damage;

        // HeadCollider 2배 데미지
        if (collision.collider.CompareTag("Head"))
        {
            finalDamage *= damage;
        }

        // 체력 차감
        target.TakeDamage(finalDamage);

        // 화살 제거
        Destroy(gameObject);
    }
}
