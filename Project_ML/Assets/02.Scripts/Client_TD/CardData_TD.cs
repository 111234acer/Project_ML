using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData_TD
{
    [Header("공통 정보")]
    public string id;                           // 고유 카드 ID
    public CardType cardType;                   // 카드 타입
    public string cardName;                     // 카드 이름
    [TextArea] public string description;       // 카드 설명

    [Header("카드 스탯용")]
    public StatType statType;                   // 스탯 타입 : 강화할 스탯 종류
    public float statValue;                     // 강화 수치

    [Header("스킬 카드용")]
    public string skillId;                      // 스킬 식별자 

    [Header("힐 카드용")]
    public float healAmount = 50f;              // 타워에 힐
}
