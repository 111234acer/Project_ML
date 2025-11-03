using UnityEngine;
using Photon.Pun;

public class HealOrb_Net : MonoBehaviourPun
{
    public int healAmount = 60;
    public float lifeTime = 5f;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var hp = other.GetComponent<PlayerHealth_Server>();
        if (hp != null && !hp.isDead)
        {
            // ÆÀ ±¸ºÐÀ» ¾²¸é °°Àº ÆÀ¸¸ Èú
            hp.RequestHeal(healAmount);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
