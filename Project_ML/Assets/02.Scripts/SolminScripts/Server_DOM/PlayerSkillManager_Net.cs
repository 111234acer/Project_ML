using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerSkillManager_Net : MonoBehaviourPun
{
    [Header("Skill References")]
    public PlayerSkill_Net skillMouse1;
    public PlayerSkill_Net skillShift;
    public PlayerSkill_Net skillR;

    // 전역 스킬 사용 잠금 상태
    public static bool IsUsingAnySkill { get; private set; } = false;

    // 스킬 이름으로 관리
    private Dictionary<string, PlayerSkill_Net> skillDict = new();

    void Awake()
    {
        // 이름 기반 등록 (UI slotName과 일치해야 함)
        if (skillMouse1 != null) skillDict["Mouse1"] = skillMouse1;
        if (skillShift != null) skillDict["Shift"] = skillShift;
        if (skillR != null) skillDict["R"] = skillR;
    }

    void Update()
    {
        if (!photonView.IsMine || IsUsingAnySkill) return;

        // 입력 처리
        if (Input.GetKeyDown(KeyCode.Mouse1) && skillMouse1 != null) skillMouse1.RequestUse();
        if (Input.GetKeyDown(KeyCode.LeftShift) && skillShift != null) skillShift.RequestUse();
        if (Input.GetKeyDown(KeyCode.R) && skillR != null) skillR.RequestUse();
    }

    // 스킬 사용 요청 (클라 → 서버)
    public void RequestSkillUse(string key)
    {
        if (!skillDict.ContainsKey(key)) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // 내가 서버면 직접 실행
            skillDict[key].Activate();
        }
        else
        {
            // 클라이언트면 서버에 요청
            photonView.RPC(nameof(Server_RequestSkillUse), RpcTarget.MasterClient, key);
        }
    }

    // 서버에서 실제 Activate 실행
    [PunRPC]
    void Server_RequestSkillUse(string key)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (skillDict.TryGetValue(key, out var skill))
            skill.Activate();
    }

    // 스킬 잠금 상태 수동 설정
    public static void SetSkillLock(bool active)
    {
        IsUsingAnySkill = active;
    }
}
