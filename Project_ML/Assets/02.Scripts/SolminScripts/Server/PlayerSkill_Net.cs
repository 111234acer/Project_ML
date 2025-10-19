using UnityEngine;
using Photon.Pun;

public abstract class PlayerSkill_Net : MonoBehaviourPun
{
    [Header("Skill Info")]
    public string skillName;          // 스킬 이름 (UI 식별용)
    public float cooldown = 5f;       // 쿨다운
    protected float nextUseTime = 0f; // 다음 사용 가능 시간

    // 실제 스킬 효과 (서버에서만 실행)
    public abstract void Activate();

    // 클라에서 스킬 사용 요청
    public void RequestUse()
    {
        // 쿨타임 체크
        if (Time.time < nextUseTime) return;
        nextUseTime = Time.time + cooldown;

        // PlayerSkillManager_Net에 위임
        var mgr = GetComponent<PlayerSkillManager_Net>();
        if (mgr != null)
        {
            mgr.RequestSkillUse(skillName);
        }
    }

    // UI용 쿨다운 비율
    public float GetCooldownPercent()
    {
        return Mathf.Clamp01((nextUseTime - Time.time) / cooldown);
    }

    // 모든 스킬 종료 시 호출 -> 공격/이동 다시 가능하게
    protected void EndSkill()
    {
        PlayerSkillManager_Net.SetSkillLock(false);
    }
}
