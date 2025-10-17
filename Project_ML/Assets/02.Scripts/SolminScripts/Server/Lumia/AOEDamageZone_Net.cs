using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AOEDamageZone_Net : MonoBehaviourPun
{
    public float radius = 5f;
    public float damagePerSecond = 30f;
    public float duration = 5f;
    public LayerMask targetLayer;

    public void Initialize(float dps, float dur)
    {
        damagePerSecond = dps;
        duration = dur;

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(DamageRoutine());
    }

    IEnumerator DamageRoutine()
    {
        float tick = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayer);
            foreach (var hit in hits)
            {
                var hp = hit.GetComponent<PlayerHealth_Server>();
                if (hp != null && !hp.isDead)
                {
                    hp.photonView.RPC("Server_ApplyDamage", RpcTarget.MasterClient, Mathf.RoundToInt(damagePerSecond));
                }
            }

            elapsed += tick;
            yield return new WaitForSeconds(tick);
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
