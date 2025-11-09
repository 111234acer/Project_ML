using UnityEngine;
using Photon.Pun;

public class AOEDamageZone_CP : MonoBehaviour
{
    int ownerViewID;
    float radius;
    float damagePerSec;
    float lifeTime;
    float tick = 1f;

    public void Init(int ownerViewID, float radius, float dps, float lifeTime)
    {
        this.ownerViewID = ownerViewID;
        this.radius = radius;
        this.damagePerSec = dps;
        this.lifeTime = lifeTime;

        InvokeRepeating(nameof(DoDamage), 0f, tick);
        Destroy(gameObject, lifeTime);
    }

    void DoDamage()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, radius);
        foreach (var col in cols)
        {
            if (!col.CompareTag("Player")) continue;
            PhotonView pv = col.GetComponent<PhotonView>();
            if (pv == null) continue;
            if (pv.ViewID == ownerViewID) continue;

            pv.RPC("RPC_TakeDamage", pv.Owner, damagePerSec);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
