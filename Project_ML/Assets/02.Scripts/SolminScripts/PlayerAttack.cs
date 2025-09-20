using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAttack : MonoBehaviour
{
    [Header("Lumia Settings")]
    public Transform firePoint;         // �߻� ��ġ
    public float fireRate = 1.0f;       // ���� �ӵ�
    protected float nextFireTime = 0f;
    public float coolTime = 1f;

    public abstract void Attack();      // ���� ���� �޼���(�� ĳ���ͺ� ����)
    
    protected bool CanAttack()          // ���� ������ �������� üũ
    {
        return Time.time >= nextFireTime;
    }

    protected void UpdateFireTime()     // ���� �� ��Ÿ�� ����
    {
        nextFireTime = Time.time + coolTime / fireRate;   
    }
}
