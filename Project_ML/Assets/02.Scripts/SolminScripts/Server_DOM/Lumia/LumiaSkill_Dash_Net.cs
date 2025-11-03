using System.Collections;
using UnityEngine;
using Photon.Pun;

// [루미아 스킬2: 대시]
// 짧은 시간 동안 전방으로 돌진.
// 서버에서 이동 처리 (ServerMotor와 권위 일치)
// 클라 애니메이션, HUD 쿨다운 자동 반영.
[DisallowMultipleComponent]
public class LumiaSkill_Dash_Net : PlayerSkill_Net
{
    [Header("Dash Settings")]
    public float dashSpeed = 4f;
    public float duration = 0.2f;

    private CharacterController cc;
    private AnimationHandler anim;

    void Awake()
    {
        skillName = "대시";
        cooldown = 7f;

        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<AnimationHandler>();
    }

    public override void Activate()
    {
        if (!photonView.IsMine) return;

        photonView.RPC(nameof(Client_Anim_Skill2), RpcTarget.All);
        photonView.RPC(nameof(Server_DoDash), RpcTarget.MasterClient);
    }

    [PunRPC]
    void Server_DoDash()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        float t = 0f;
        while (t < duration)
        {
            if (cc != null)
                cc.Move(transform.forward * dashSpeed * Time.fixedDeltaTime);
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        photonView.RPC(nameof(Client_DashEnd), RpcTarget.All);
    }

    [PunRPC] void Client_Anim_Skill2() => anim?.Skill2Trigger();
    [PunRPC] void Client_DashEnd() => EndSkill(); // 쿨다운 자동
}
