using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSkill : MonoBehaviour
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
        if(Time.time >= nextUseTime)
        {
            Activate();
            nextUseTime = Time.time + cooldown;
        }
        else
        {
            Debug.Log($"{skillName} 쿨타임 중... {Mathf.Ceil(nextUseTime - Time.time)}초 남음");
        }
    }

    // 쿨타임 진행 비율 (UI)
    public float GetCooldownPercent()
    {
        return Mathf.Clamp01((nextUseTime - Time.time)/cooldown);
    }
}
