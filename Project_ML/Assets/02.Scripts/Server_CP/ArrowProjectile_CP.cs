using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class ArrowProjectile_CP : MonoBehaviour
{
    int ownerViewID;
    float damage;
    public float lifeTime = 6f;

    public void Init(int ownerViewID, float damage)
    {
        this.ownerViewID = ownerViewID;
        this.damage = damage;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null) return;
        if (pv.ViewID == ownerViewID) return; // 자기 자신 제외

        pv.RPC("RPC_TakeDamage", pv.Owner, damage);
        Destroy(gameObject);
    }
}
