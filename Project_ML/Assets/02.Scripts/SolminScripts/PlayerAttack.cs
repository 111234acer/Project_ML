using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAttack : MonoBehaviour
{
    [Header("Lumia Settings")]
    public Transform firePoint;         // 발사 위치
    public float fireRate = 1.0f;       // 공격 속도
    protected float nextFireTime = 0f;
    public float coolTime = 1f;

    public abstract void Attack();      // 공격 실행 메서드(각 캐릭터별 구현)
    
    protected bool CanAttack()          // 현재 공격이 가능한지 체크
    {
        return Time.time >= nextFireTime;
    }

    protected void UpdateFireTime()     // 공격 후 쿨타임 갱신
    {
        nextFireTime = Time.time + coolTime / fireRate;   
    }
}
