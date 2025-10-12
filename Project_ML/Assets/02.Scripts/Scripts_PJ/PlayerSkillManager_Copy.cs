using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSkillManager_Copy : MonoBehaviour
{
    PlayerHealth_Copy health;
    PhotonView pv;

    public PlayerSkill_Copy skill1;  // Q
    public PlayerSkill_Copy skill2;  // Shift
    public PlayerSkill_Copy ultimate; // R

    [HideInInspector] public float suppressFire1Until = 0f;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        health = GetComponent<PlayerHealth_Copy>();
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom && pv && !pv.IsMine) return;
        if (health != null && health.isDead) return;

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (skill1 == null || skill1.GetCooldownPercent() > 0f) return;
            pv.RPC("RPC_UseSkill", RpcTarget.All, 1, Time.time);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (skill2 == null || skill2.GetCooldownPercent() > 0f) return;
            pv.RPC("RPC_UseSkill", RpcTarget.All, 2, Time.time);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            var ult = ultimate as LumiaSkill_Ult_Copy;
            if (ult != null) ult.Activate();
            else
            {
                pv.RPC("RPC_UseSkill", RpcTarget.All, 3, Time.time);
            }
        }
    }

    [PunRPC]
    void RPC_UseSkill(int slot, float baseTime)
    {
        switch (slot)
        {
            case 1: if (skill1) skill1.NetworkTrigger(baseTime); break;
            case 2: if (skill2) skill2.NetworkTrigger(baseTime); break;
            case 3: if (ultimate) ultimate.NetworkTrigger(baseTime); break;
        }
    }
}
