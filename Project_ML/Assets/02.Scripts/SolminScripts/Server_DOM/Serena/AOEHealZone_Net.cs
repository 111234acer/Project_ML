using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AOEHealZone_Net : MonoBehaviourPun
{
    public float radius = 5f;
    public int healPerSecond = 30;
    public float duration = 5f;
    public LayerMask targetLayer; // Player ·¹ÀÌ¾î

    public void Initialize(int hps, float dur)
    {
        healPerSecond = hps;
        duration = dur;
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(HealRoutine());
    }

    IEnumerator HealRoutine()
    {
        float tick = 1f, elapsed = 0f;
        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayer);
            foreach (var hit in hits)
            {
                var hp = hit.GetComponent<PlayerHealth_Server>();
                if (hp != null && !hp.isDead)
                {
                    hp.RequestHeal(healPerSecond);
                }
            }
            elapsed += tick;
            yield return new WaitForSeconds(tick);
        }

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}