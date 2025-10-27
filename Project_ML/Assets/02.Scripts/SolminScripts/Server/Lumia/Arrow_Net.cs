using UnityEngine;
using Photon.Pun;

public class Arrow_Net : MonoBehaviourPun
{
    [Header("Arrow Settings")]
    public float lifeTime = 5f;                     // 자동 소멸 시간
    public int damage = 60;                         // 화살 데미지 60

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 클라이언트에서 물리처리 서버에서 로직 실행
        if (!PhotonNetwork.IsMasterClient) return;

        // 피격 대상 확인
        var hp = collision.collider.GetComponent<PlayerHealth_Server>();
        if (hp != null && !hp.isDead)
        {
            // 데미지만 실행
            hp.photonView.RPC("Server_ApplyDamage", RpcTarget.MasterClient, damage);
        } 

        // 충돌 대상이 누구든지 삭제
        PhotonNetwork.Destroy(gameObject);
    }
}
