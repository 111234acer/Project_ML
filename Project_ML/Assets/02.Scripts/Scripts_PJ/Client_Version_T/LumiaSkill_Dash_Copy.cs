using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LumiaSkill_Dash_Copy : PlayerSkill_Copy
{
    [Header("Dash Settings")]
    public float dashDistance = 4f;     // 대시 거리
    public float dashDuration = 0.2f;   // 대시에 걸리는 시간(짧을수록 빨라짐)

    private CharacterController controller;
    private PlayerController_Copy pc;

    PhotonView pv;

    private void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        skillName = "루미아 대시";
        cooldown = 6f;    // 쿨타임
        controller = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController_Copy>();
    }

    public override void Activate()                             // 스킬 발동
    {
        if (pv != null && pv.IsMine)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        Vector3 dir = (pc && pc.lastMoveWorldDir.sqrMagnitude > 0.0001f)
                       ? pc.lastMoveWorldDir.normalized
                       : Vector3.zero;

        if (dir.sqrMagnitude < 0.0001f)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            dir = (transform.right * h + transform.forward * v);
        }
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;

        dir.y = 0f; dir.Normalize();

        float speed = dashDistance / dashDuration;
        float t = 0f;
        pc.isDashing = true;
        while (t < dashDuration)
        {
            controller.Move(dir * speed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
        pc.isDashing = false;
    }
}
