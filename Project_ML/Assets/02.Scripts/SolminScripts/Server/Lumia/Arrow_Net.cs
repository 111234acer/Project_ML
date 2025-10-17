using UnityEngine;
using Photon.Pun;

public class Arrow_Net : MonoBehaviourPun
{
    public int damage = 20;
    public float lifeTime = 5f;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var hp = other.GetComponent<PlayerHealth_Server>();
        if (hp != null && !hp.isDead)
        {
            hp.photonView.RPC("Server_ApplyDamage", RpcTarget.MasterClient, damage);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
