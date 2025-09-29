using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public Slider healthSlider;         // hp바
    public TMP_Text healthText;         // 체력Text

    public SkillSlot[] skillSlots;

    private PlayerHealth health;
    private PlayerSkillManager skillMgr;

    [System.Serializable]
    public class SkillSlot
    {
        public string slotName;        // 조작키
        public Image cooldownMask;
        public TMP_Text cooldownText;
        [HideInInspector] public PlayerSkill skill; // 런타임에 매핑됨
    }
    public void Init(PlayerHealth h, PlayerSkillManager mgr)
    {
        health = h;
        skillMgr = mgr;

        MapSkills();
        RefreshHealth();
        RefreshSkills();
    }

    void Update()
    {
        if (health != null)
        {
            RefreshHealth();
        }
        if (skillSlots != null && skillSlots.Length > 0)
        { 
            RefreshSkills();
        }
    }

    void RefreshHealth()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = health.MaxHealth;
            healthSlider.value = health.currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = health.currentHealth + "/" + health.MaxHealth;
        }
    }

    void RefreshSkills()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            var s = skillSlots[i].skill;
            var m = skillSlots[i].cooldownMask;
            var t = skillSlots[i].cooldownText;

            if (s == null) continue;

            float percent = s.GetCooldownPercent();
            if (m != null) m.fillAmount = percent;

            if (t != null)
            {
                if (percent > 0f)
                {
                    float remain = Mathf.Ceil(s.cooldown * percent);
                    t.text = remain.ToString("0");
                }
                else
                {
                    t.text = "";
                }
            }
        }
    }

    void MapSkills()
    {
        if (skillMgr == null || skillSlots == null) return;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i].skill != null) continue;

            string n = skillSlots[i].slotName;

            if(n == "Q")
            {
                skillSlots[i].skill = skillMgr.skill1;
            }
            else if(n == "Shift")
            {
                skillSlots[i].skill = skillMgr.skill2;
            }
            else if(n == "R")
            {
                skillSlots[i].skill = skillMgr.ultimate;
            }
        }
    }
}