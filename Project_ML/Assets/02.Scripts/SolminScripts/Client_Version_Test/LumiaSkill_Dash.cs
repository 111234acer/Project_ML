using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaSkill_Dash : PlayerSkill
{
    [Header("Dash Settings")]
    public float dashDistance = 4f;     // 대시 거리
    public float dashDuration = 0.2f;   // 대시에 걸리는 시간(짧을수록 빨라짐)
    private CharacterController controller;
    private PlayerController playerController;

    private void Awake()
    {
        skillName = "루미아 대시";
        cooldown = 6f;    // 쿨타임
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    public override void Activate()                             // 스킬 발동
    {
        if(controller != null && !playerController.isDashing) 
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        playerController.isDashing = true;

        float h = Input.GetAxisRaw("Horizontal");       // AD
        float v = Input.GetAxisRaw("Vertical");         // WS

        Vector3 dashDir = transform.forward;            // 기본 대시는 플레이어가 보는 정면 방향
        
        if(h != 0 ||  v != 0)
        {
            dashDir = (transform.right * h + transform.forward * v).normalized;
        }

        float elapsed = 0f;         // 경과 시간 누적

        while(elapsed < dashDuration)       // 총 거리(dashDistance)를 총 시간(dashDuration)에 맞춰 나눠 이동
        {
            float step = (dashDistance/dashDuration) * Time.deltaTime;
            controller.Move(dashDir *  step);
            elapsed += Time.deltaTime;      //  경과 시간
            yield return null;
        }

        playerController.isDashing = false;
    }
}
