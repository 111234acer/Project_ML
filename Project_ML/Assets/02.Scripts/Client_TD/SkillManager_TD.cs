// SkillManager_TD.cs
// 자동 발동 스킬 관리 매니저
// 스킬 등록 / 강화 / 쿨타임 체크 / 자동 발동 담당
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillData_TD
{
    [Header("기본 정보")]
    public string skillId;                 // 스킬 식별 ID (ex: "Fireball")
    public string skillName;               // 표시용 이름
    public GameObject skillEffectPrefab;   // 발동 시 생성할 이펙트 프리팹
    public float baseCooldown = 5f;        // 기본 쿨타임
    public float cooldown;                 // 현재 적용 쿨타임 (레벨 보정 반영)
    public int level = 1;                  // 현재 스킬 레벨
    public int maxLevel = 5;               // 최대 강화 레벨
    public float damageMultiplier = 1f;    // 데미지 배율 (필요시 공격력 연동 가능)

    // 내부용: 쿨타임 타이머
    [HideInInspector] public float timer = 0f;
}

public class SkillManager_TD : MonoBehaviour
{
    [Header("참조 설정")]
    public Transform firePoint;        // 스킬 발사 기준 위치
    public Camera playerCamera;        // 방향 참조용 카메라 (PlayerLook과 동일)
    public PlayerAttack_TD playerAttack;  // 공격력 / 크리티컬 등 참조 가능 (선택)

    [Header("스킬 목록")]
    public List<SkillData_TD> activeSkills = new List<SkillData_TD>();  // 현재 활성 스킬 목록

    // Unity Loop
    void Update()
    {
        if (activeSkills.Count == 0) return;

        foreach (SkillData_TD skill in activeSkills)
        {
            skill.timer += Time.deltaTime;

            // 쿨타임이 다 돌면 스킬 자동 발동
            if (skill.timer >= skill.cooldown)
            {
                ActivateSkill(skill);
                skill.timer = 0f; // 타이머 초기화
            }
        }
    }

    // 스킬 발동 처리
    private void ActivateSkill(SkillData_TD skill)
    {
        if (!skill.skillEffectPrefab || !firePoint || !playerCamera)
        {
            Debug.LogWarning($"[SkillManager_TD] 스킬 {skill.skillName} 발동 실패 (참조 누락)");
            return;
        }

        // 카메라 forward 기준으로 방향 지정
        Vector3 dir = playerCamera.transform.forward;
        Quaternion rot = Quaternion.LookRotation(dir);

        // 스킬 이펙트 생성
        Instantiate(skill.skillEffectPrefab, firePoint.position, rot);

        Debug.Log($"[SkillManager_TD] 스킬 발동: {skill.skillName} (Lv.{skill.level})");
    }

    // ==========================================================
    // 외부 인터페이스: 카드 시스템에서 호출
    // ==========================================================

    /// <summary>
    /// 스킬을 새로 추가하거나 기존 스킬 레벨을 강화한다.
    /// (CardManager_TD에서 호출)
    /// </summary>
    public void AddOrLevelUpSkill(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) return;

        // 이미 존재하는 스킬 찾기
        SkillData_TD existing = activeSkills.Find(s => s.skillId == skillId);

        if (existing != null)
        {
            // 이미 가진 스킬이면 강화
            if (existing.level < existing.maxLevel)
            {
                existing.level++;
                existing.damageMultiplier += 0.2f;  // 데미지 증가
                existing.cooldown = Mathf.Max(existing.baseCooldown - (existing.level * 0.5f), 1f); // 쿨타임 단축
                Debug.Log($"[SkillManager_TD] 스킬 강화: {existing.skillName} → Lv.{existing.level}");
            }
            else
            {
                Debug.Log($"[SkillManager_TD] {existing.skillName} 은(는) 이미 최대 레벨입니다.");
            }
        }
        else
        {
            // 신규 스킬 추가
            SkillData_TD newSkill = CreateSkillData(skillId);
            if (newSkill != null)
            {
                activeSkills.Add(newSkill);
                Debug.Log($"[SkillManager_TD] 새로운 스킬 추가: {newSkill.skillName} (Lv.1)");
            }
            else
            {
                Debug.LogWarning($"[SkillManager_TD] {skillId} 스킬 정보를 찾을 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 카드 매니저에서 5강 스킬 필터링 시 사용
    /// </summary>
    public int GetSkillLevel(string skillId)
    {
        SkillData_TD skill = activeSkills.Find(s => s.skillId == skillId);
        return (skill != null) ? skill.level : 0;
    }

    public int GetSkillMaxLevel(string skillId)
    {
        SkillData_TD skill = activeSkills.Find(s => s.skillId == skillId);
        return (skill != null) ? skill.maxLevel : 5;
    }

    // 스킬 생성 헬퍼 (새 스킬 등록용)

    private SkillData_TD CreateSkillData(string skillId)
    {
        SkillData_TD skill = new SkillData_TD();

        // 여기서 skillId 기준으로 스킬 세부정보를 지정한다.
        // (추후 ScriptableObject로 관리해도 좋음)

        switch (skillId)
        {
            case "Meteor":
                skill.skillId = skillId;
                skill.skillName = "메테오";
                skill.skillEffectPrefab = Resources.Load<GameObject>("Effects/Meteor");
                skill.baseCooldown = 10f;
                break;

            case "Fireball":
                skill.skillId = skillId;
                skill.skillName = "파이어볼";
                skill.skillEffectPrefab = Resources.Load<GameObject>("Effects/Fireball");
                skill.baseCooldown = 4f;
                break;

            case "FlamePillar":
                skill.skillId = skillId;
                skill.skillName = "불기둥";
                skill.skillEffectPrefab = Resources.Load<GameObject>("Effects/FlamePillar");
                skill.baseCooldown = 8f;
                break;

            case "Lightning":
                skill.skillId = skillId;
                skill.skillName = "라이트닝";
                skill.skillEffectPrefab = Resources.Load<GameObject>("Effects/Lightning");
                skill.baseCooldown = 6f;
                break;

            case "ManaBolt":
                skill.skillId = skillId;
                skill.skillName = "마나볼트";
                skill.skillEffectPrefab = Resources.Load<GameObject>("Effects/ManaBolt");
                skill.baseCooldown = 3f;
                break;

            case "IceBolt":
                skill.skillId = skillId;
                skill.skillName = "아이스볼트";
                skill.skillEffectPrefab = Resources.Load<GameObject>("Effects/IceBolt");
                skill.baseCooldown = 5f;
                break;

            case "RockThrow":
                skill.skillId = skillId;
                skill.skillName = "바위 던지기";
                skill.skillEffectPrefab = Resources.Load<GameObject>("Effects/RockThrow");                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
                skill.baseCooldown = 7f;
                break;

            default:
                return null;
        }

        // 기본값 초기화
        skill.cooldown = skill.baseCooldown;
        skill.level = 1;
        skill.damageMultiplier = 1f;
        skill.timer = 0f;

        return skill;
    }
}
