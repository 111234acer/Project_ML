using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerSkillManager_Net : MonoBehaviourPun
{
    public PlayerSkill_Net skillMouse1;
    public PlayerSkill_Net skillShift;
    public PlayerSkill_Net skillR;

    // 전역 스킬 사용 상태
    public static bool IsUsingAnySkill { get; private set; } = false;

    void Update()
    {
        if (!photonView.IsMine) return;

        // 마우스 우클릭
        if (Input.GetKeyDown(KeyCode.Mouse1))
            skillMouse1?.RequestUse();

        // Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
            skillShift?.RequestUse();

        // R
        if (Input.GetKeyDown(KeyCode.R))
            skillR?.RequestUse();
    }

    void TryUseSkill(PlayerSkill_Net skill)
    {
        if(skill == null) return;

        // 아마 스킬 사용 중이면 무시
        if (IsUsingAnySkill) return;

        StartCoroutine(UseSkillRoutine(skill));
    }

    IEnumerator UseSkillRoutine(PlayerSkill_Net skill)
    {
        // 스킬 사용 시작 → 공격 잠금
        IsUsingAnySkill = true;

        skill.RequestUse();

        // 기본적으로 짧은 딜레이 후 자동 해제
        // (스킬 내부에서 EndSkill()을 호출하면 더 일찍 풀림)
        yield return new WaitForSeconds(0.25f);

        IsUsingAnySkill = false;
    }

    // 외부에서도 스킬 잠금 수동 제어 가능
    public static void SetSkillLock(bool active)
    {
        IsUsingAnySkill = active;
    }
}
