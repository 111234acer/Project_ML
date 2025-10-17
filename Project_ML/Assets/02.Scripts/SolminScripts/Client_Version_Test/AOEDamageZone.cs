using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEDamageZone : MonoBehaviour
{
    private float damagePerSecond;
    private float duration;

    public void Initialize(float dps , float dur)
    {
        damagePerSecond = dps;
        duration = dur;
        StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        float timer = 0;
        float tick = 1f;    // 1초 간격으로 피해 적용

        while(timer < duration)
        {
            // 범위 내 적 탐색
            Collider[] hits = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Player"));
            
            foreach(Collider hit in hits)
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if(ph != null && !ph.IsDead())
                {
                    ph.TakeDamage(Mathf.RoundToInt(damagePerSecond));
                }
            }

            timer += tick;
            yield return new WaitForSeconds(tick);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
