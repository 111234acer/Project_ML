using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PlayerInput_CP))]
public class PlayerSkillManager_CP : MonoBehaviourPun
{
    public PlayerSkillBase_CP skill1;   // ¿ìÅ¬¸¯
    public PlayerSkillBase_CP dash;     // Shift
    public PlayerSkillBase_CP ultimate; // R

    PlayerInput_CP input;

    void Awake()
    {
        input = GetComponent<PlayerInput_CP>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (input.Skill1 && skill1 && skill1.CanUse())
            skill1.Use();

        if (input.dash && dash && dash.CanUse())
            dash.Use();

        if (input.ultimate && ultimate && ultimate.CanUse())
            ultimate.Use();
    }
}
