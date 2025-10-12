using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSkill_Copy : MonoBehaviour
{
    [Header("Skill Info")]
    public string skillName;            // 스킬 이름
    public float cooldown = 5f;         // 스킬 쿨타임
    private float nextUseTime = 0f;     // 다음 사용 가능 시간

    // 실제 스킬 효과(각 스킬에서 구현)
    public abstract void Activate();

    // 스킬 사용 함수 (쿨타임 체크)
    public void Use()
    {
        if (Time.time >= nextUseTime)
        {
            Activate();
            nextUseTime = Time.time + cooldown;
        }
    }
    public void NetworkTrigger(float baseTime)
    {
        if (baseTime < nextUseTime) return;

        nextUseTime = baseTime + cooldown;
        Activate();
    }

    // 쿨타임 진행 비율 (UI)
    public float GetCooldownPercent()
    {
        return Mathf.Clamp01((nextUseTime - Time.time) / cooldown);
    }

    public void ForceStartCooldown(float baseTime)
    {
        // baseTime 기준으로 즉시 쿨 시작
        // (예: 발동 확정 시점 Time.time)
        var start = (baseTime <= 0f) ? Time.time : baseTime;
        // nextUseTime은 원래 private, 이 메서드 안에서만 만짐
        typeof(PlayerSkill_Copy)
            .GetField("nextUseTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(this, start + cooldown);
    }
}
