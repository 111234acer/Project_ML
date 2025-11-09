using UnityEngine;
using Photon.Pun;

public class PlayerHealth_CP : MonoBehaviourPun
{
    public float maxHP = 100f;
    [HideInInspector] public float currentHP;
    bool isDead;
    Animator anim;

    void Start()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();
    }

    [PunRPC]
    public void RPC_TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim?.SetTrigger("dieTrigger");

        var ctrl = GetComponent<PlayerController_CP>();
        if (ctrl) ctrl.enabled = false;

        var atk = GetComponent<LumiaAttack_CP>();
        if (atk) atk.enabled = false;

        var skill = GetComponent<PlayerSkillManager_CP>();
        if (skill) skill.enabled = false;

        if (photonView.IsMine && GameManager_CP.Instance != null)
            GameManager_CP.Instance.RequestRespawn_CP(this);
    }

    public void ReviveAt(Vector3 pos)
    {
        transform.position = pos;
        currentHP = maxHP;
        isDead = false;

        var ctrl = GetComponent<PlayerController_CP>();
        if (ctrl) ctrl.enabled = true;

        var atk = GetComponent<LumiaAttack_CP>();
        if (atk) atk.enabled = true;

        var skill = GetComponent<PlayerSkillManager_CP>();
        if (skill) skill.enabled = true;
    }
}
