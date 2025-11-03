using UnityEngine;
using Photon.Pun;


// 모든 캐릭터의 기본 공격용 공통 베이스 클래스.
// 쿨다운 및 공격 가능 여부만 관리.
// 실제 공격(발사, 스킬 등)은 자식 클래스에서 구현.

public abstract class PlayerAttack_Net : MonoBehaviourPun
{
    [Header("Common Attack Settings")]
    public float cooldown = 0.6f;           // 공격 쿨다운 (기본 0.6초)
    protected float nextFireTime = 0f;      // 다음 공격 가능 시간


    // 지금 공격할 수 있는지 확인
    protected bool CanAttack()
    {
        return Time.time >= nextFireTime;
    }

    // 공격 후 쿨다운 초기화
    protected void ResetCooldown()
    {
        nextFireTime = Time.time + cooldown;
    }


    // 실제 공격 로직은 각 캐릭터에서 구현
    public abstract void PerformAttack();
}
