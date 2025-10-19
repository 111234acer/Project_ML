using UnityEngine;
using Photon.Pun;

public abstract class PlayerAttack_Net : MonoBehaviourPun
{
    [Header("Attack Settings")]
    public Transform firePoint;           // 발사 위치
    public float fireRate = 1f;           // 공격 속도
    public float coolDown = 1f;           // 쿨타임
    protected float nextFireTime = 0f;    // 다음 공격 가능 시간

    // 공격 가능한지 확인
    protected bool CanAttack()
    {
        return Time.time >= nextFireTime;
    }

    // 쿨타임 갱신
    protected void UpdateFireTime()
    {
        nextFireTime = Time.time + coolDown / fireRate;
    }

    // 클라이언트 -> 서버 공격 요청
    public void RequestAttack(float charge)
    {
        if (!photonView.IsMine) return; // 본인 캐릭터만 공격 가능
        photonView.RPC(nameof(Server_Attack), RpcTarget.MasterClient, charge);
    }

    // 서버에서 실행되는 공격 (권한 보유자만)
    [PunRPC]
    protected virtual void Server_Attack(float charge, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return; // 서버(마스터)만 허용
        if (!CanAttack()) return;

        Attack(charge); // 하위 클래스의 공격 실행 (ex. 화살, 대시, 궁극기)
        photonView.RPC(nameof(Client_OnAttack), RpcTarget.All, charge);
        UpdateFireTime();
    }

    // 클라이언트 표시용 (이펙트, 사운드 등)
    [PunRPC]
    protected virtual void Client_OnAttack(float charge)
    {
        // 모든 클라이언트에 시각적 효과 표시 가능
    }

    // 하위 클래스 (예 : 루미아)에서 구현할 실제 공격 로직
    protected abstract void Attack(float charge);
}
