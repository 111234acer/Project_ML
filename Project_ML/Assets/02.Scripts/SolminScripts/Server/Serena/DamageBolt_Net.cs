using UnityEngine;
using Photon.Pun;

public class DamageBolt_Net : MonoBehaviourPun
{
    public int damage = 40;
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
            // 팀 구분을 쓰면 여기서 적 팀만 통과
            hp.Server_ApplyDamage(damage, default);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
