using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public PlayerSkill skill1;  // QŰ
    public PlayerSkill skill2;  // ShiftŰ
    public PlayerSkill ultimate; // RŰ

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            skill1.Use();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            skill2.Use();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ultimate.Use();
        }
    }
}
