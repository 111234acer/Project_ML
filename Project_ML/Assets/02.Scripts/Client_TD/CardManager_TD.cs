// ==========================================================
// CardManager_TD.cs
// 웨이브 종료 후 3장의 카드 중 하나를 선택하여 강화/스킬/힐 적용
// ==========================================================
using System.Collections.Generic;
using UnityEngine;

public class CardManager_TD : MonoBehaviour
{
    [Header("참조")]
    public PlayerAttack_TD playerAttack;          // 플레이어 공격 (데미지, 치명타 등)
    public TowerManager_TD towerManager;          // 타워 관리 (힐 카드용)
    public SkillManager_TD skillManager;          // 스킬 관리 (스킬 카드용)
    public SingleGameManager_TD singleGameManager; // 게임 일시정지 제어용

    [Header("카드 풀")]
    public List<CardData_TD> statCards = new List<CardData_TD>();
    public List<CardData_TD> skillCards = new List<CardData_TD>();
    public List<CardData_TD> healCards = new List<CardData_TD>();

    private List<CardData_TD> currentChoices = new List<CardData_TD>();

    [Header("카드 확률 설정")]
    [Range(0f, 100f)] public float statRate = 35f;
    [Range(0f, 100f)] public float skillRate = 45f;
    [Range(0f, 100f)] public float healRate = 20f;

    // ==========================================================
    // 카드 선택 호출
    // ==========================================================
    public void ShowCardSelection()
    {
        if (singleGameManager)
            singleGameManager.SetPause(true);

        currentChoices.Clear();

        for (int i = 0; i < 3; i++)
        {
            CardData_TD card = GetRandomCard();
            if (card != null)
                currentChoices.Add(card);
        }

        Debug.Log("========== [카드 선택 시작] ==========");
        for (int i = 0; i < currentChoices.Count; i++)
        {
            var c = currentChoices[i];
            Debug.Log($"({i + 1}) {c.cardName} [{c.cardType}] - {c.description}");
        }
        Debug.Log("====================================");

        // TODO: UI 생성 (CardSelectionUI_TD)에서 버튼 3개 띄우는 부분 연결 예정
    }

    // ==========================================================
    // 카드 선택
    // ==========================================================
    public void SelectCard(int index)
    {
        if (index < 0 || index >= currentChoices.Count)
            return;

        CardData_TD selected = currentChoices[index];
        ApplyCardEffect(selected);

        Debug.Log($"[CardManager_TD] 선택된 카드: {selected.cardName}");

        // 카드 선택 후 게임 재개
        if (singleGameManager)
            singleGameManager.SetPause(false);
    }

    // ==========================================================
    // 카드 랜덤 선택 로직
    // ==========================================================
    private CardData_TD GetRandomCard()
    {
        float roll = Random.Range(0f, 100f);
        CardType type;

        if (roll < statRate)
            type = CardType.Stat;
        else if (roll < statRate + skillRate)
            type = CardType.Skill;
        else
            type = CardType.Heal;

        switch (type)
        {
            case CardType.Stat:
                if (statCards.Count == 0) return null;
                return statCards[Random.Range(0, statCards.Count)];

            case CardType.Skill:
                List<CardData_TD> availableSkills = GetAvailableSkillCards();
                if (availableSkills.Count == 0)
                {
                    // 모든 스킬이 5강이면 스탯 카드로 대체
                    if (statCards.Count > 0)
                        return statCards[Random.Range(0, statCards.Count)];
                    return null;
                }
                return availableSkills[Random.Range(0, availableSkills.Count)];

            case CardType.Heal:
                if (healCards.Count == 0) return null;
                return healCards[Random.Range(0, healCards.Count)];
        }

        return null;
    }

    // ==========================================================
    // 5레벨 이하인 스킬만 선택 가능
    // ==========================================================
    private List<CardData_TD> GetAvailableSkillCards()
    {
        List<CardData_TD> result = new List<CardData_TD>();

        foreach (var card in skillCards)
        {
            if (skillManager == null)
            {
                result.Add(card);
            }
            else
            {
                int currentLvl = skillManager.GetSkillLevel(card.skillId);
                int maxLvl = skillManager.GetSkillMaxLevel(card.skillId);

                if (currentLvl < maxLvl)
                    result.Add(card);
            }
        }

        return result;
    }

    // ==========================================================
    // 카드 효과 적용
    // ==========================================================
    private void ApplyCardEffect(CardData_TD card)
    {
        switch (card.cardType)
        {
            case CardType.Stat:
                ApplyStatCard(card);
                break;

            case CardType.Skill:
                if (skillManager != null)
                    skillManager.AddOrLevelUpSkill(card.skillId);
                break;

            case CardType.Heal:
                if (towerManager != null)
                {
                    towerManager.Recover(card.healAmount); // 고정 회복 수치
                    Debug.Log($"[CardManager_TD] 힐 카드 사용: +{card.healAmount} 회복");
                }
                break;
        }
    }

    // ==========================================================
    // 스탯 카드 효과 적용
    // ==========================================================
    private void ApplyStatCard(CardData_TD card)
    {
        if (playerAttack == null)
        {
            Debug.LogWarning("[CardManager_TD] PlayerAttack 참조 누락");
            return;
        }

        switch (card.statType)
        {
            case StatType.Damage:
                playerAttack.baseDamage += card.statValue;
                Debug.Log($"[CardManager_TD] 공격력 +{card.statValue}");
                break;

            case StatType.AttackSpeed:
                playerAttack.attackInterval = Mathf.Max(0.2f, playerAttack.attackInterval - card.statValue);
                Debug.Log($"[CardManager_TD] 공격 주기 -{card.statValue}초");
                break;

            case StatType.MoveSpeed:
                var pc = playerAttack.GetComponent<PlayerController_TD>();
                if (pc)
                {
                    pc.moveSpeed += card.statValue;
                    Debug.Log($"[CardManager_TD] 이동속도 +{card.statValue}");
                }
                break;

            case StatType.CritChance:
                playerAttack.criticalChance = Mathf.Min(100f, playerAttack.criticalChance + card.statValue);
                Debug.Log($"[CardManager_TD] 치명타 확률 +{card.statValue}%");
                break;

            case StatType.CritDamage:
                playerAttack.criticalMultiplier += card.statValue;
                Debug.Log($"[CardManager_TD] 치명타 배율 +{card.statValue}");
                break;
        }
    }
}
