using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaAttack : PlayerAttack
{
    public GameObject arrowPrefab;                  // 화살 프리팹
    public float arrowSpeed = 20f;

    private void Update()
    {
        // 좌클릭 입력 공격 가능하면 Attack 호출
        if(Input.GetButtonDown("Fire1") && CanAttack())
        {
            Attack();
        }
    }
    public override void Attack()
    {
        if(arrowPrefab == null || firePoint == null) return;

        // 화살 생성
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        // Rigidbody 가져와서 발사 
        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        if(rb != null)
        {
            rb.useGravity = false;      // 직선 발사
            rb.velocity = firePoint.forward * arrowSpeed;
        }

        UpdateFireTime() ;
    }
}
