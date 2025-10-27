using System.Collections;
using UnityEngine;
using Photon.Pun;

public class LumiaSkill_Dash_Net : PlayerSkill_Net
{
    [Header("Dash Settings")]
    public float dashDistance = 4f;
    public float dashDuration = 0.2f;

    private CharacterController controller;
    private PlayerHealth_Server health;

    private AnimationHandler animationHandler;

    private void Awake()
    {
        skillName = "루미아 대시";
        cooldown = 6f;
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth_Server>();

        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    public override void Activate()
    {
        if (PhotonNetwork.IsMasterClient && !health.isDead)
            StartCoroutine(DashRoutine());

        photonView.RPC("Client_Anim_Skill2", RpcTarget.All);
    }

    private IEnumerator DashRoutine()
    {
        float elapsed = 0f;

        Vector3 dashDir = transform.forward;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0)
            dashDir = (transform.right * h + transform.forward * v).normalized;

        while (elapsed < dashDuration)
        {
            controller.Move(dashDir * (dashDistance / dashDuration) * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    [PunRPC] 
    void Client_Anim_Skill2()
    { 
        animationHandler?.Skill2Trigger(); 
    }
}
