using UnityEngine;
using Photon.Pun;

public class PlayerSkillManager_Net : MonoBehaviourPun
{
    public PlayerSkill_Net skillMouse1;
    public PlayerSkill_Net skillShift;
    public PlayerSkill_Net skillR;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.Mouse1))
            skillMouse1?.RequestUse();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            skillShift?.RequestUse();

        if (Input.GetKeyDown(KeyCode.R))
            skillR?.RequestUse();
    }
}
