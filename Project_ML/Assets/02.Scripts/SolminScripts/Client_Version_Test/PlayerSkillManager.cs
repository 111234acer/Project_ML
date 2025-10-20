using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public PlayerSkill skill1;  // QŰ
    public PlayerSkill skill2;  // ShiftŰ
    public PlayerSkill ultimate; // RŰ

    private AnimationHandler animationHandler;

    private void Awake()
    {
        animationHandler = GetComponentInChildren<AnimationHandler>();    
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if(skill1.Use())
            {
                animationHandler.Skill1Trigger();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(skill2.Use())
            {
                animationHandler.Skill2Trigger();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ultimate.Use();
        }
    }
}
