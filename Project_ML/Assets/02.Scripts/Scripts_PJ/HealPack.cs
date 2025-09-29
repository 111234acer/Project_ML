using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPack : MonoBehaviour
{
    [Header("Heal Settings")]
    public int healAmount = 50;          // 회복량
    public float respawnTime = 15f;      // 리스폰 시간

    private Renderer rend;

    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null && !health.IsDead())
        {
            if (health.currentHealth >= health.MaxHealth)
                return;

            health.Heal(healAmount);

            // 힐팩 비활성화 & 리스폰 시작
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        if (rend != null) rend.enabled = false;
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        if (rend != null) rend.enabled = true;
        if (col != null) col.enabled = true;
    }
}
