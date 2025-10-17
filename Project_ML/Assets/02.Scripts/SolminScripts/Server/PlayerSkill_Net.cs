using UnityEngine;
using Photon.Pun;

public abstract class PlayerSkill_Net : MonoBehaviourPun
{
    [Header("Skill Info")]
    public string skillName;
    public float cooldown = 5f;
    protected float nextUseTime = 0f;

    // 실제 스킬 효과  서버에서만 실행
    public abstract void Activate();

    // 클라에서 서버로 스킬 사용 요청
    public void RequestUse()
    {
        if (Time.time < nextUseTime) return;

        nextUseTime = Time.time + cooldown;

        if (PhotonNetwork.IsMasterClient)
        {
            Activate();
        }
        else
        {
            photonView.RPC(nameof(Server_RequestSkillUse), RpcTarget.MasterClient);
        }
    }

    // 서버에서 스킬 실행 승인
    [PunRPC]
    protected void Server_RequestSkillUse(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Activate();
    }

    // UI용 쿨다운 비율
    public float GetCooldownPercent()
    {
        return Mathf.Clamp01((nextUseTime - Time.time) / cooldown);
    }
}
