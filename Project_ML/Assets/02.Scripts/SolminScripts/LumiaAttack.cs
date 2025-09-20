using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaAttack : PlayerAttack
{
    public GameObject arrowPrefab;                  // ȭ�� ������
    public float arrowSpeed = 20f;

    private void Update()
    {
        // ��Ŭ�� �Է� ���� �����ϸ� Attack ȣ��
        if(Input.GetButtonDown("Fire1") && CanAttack())
        {
            Attack();
        }
    }
    public override void Attack()
    {
        if(arrowPrefab == null || firePoint == null) return;

        // ȭ�� ����
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        // Rigidbody �����ͼ� �߻� 
        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        if(rb != null)
        {
            rb.useGravity = false;      // ���� �߻�
            rb.velocity = firePoint.forward * arrowSpeed;
        }

        UpdateFireTime() ;
    }
}
