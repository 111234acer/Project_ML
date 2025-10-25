using UnityEngine;
using Photon.Pun;

public class SeraSkillHeal_Net : PlayerSkill_Net
{
    [Header("Heal Orb")]
    public GameObject healOrbPrefab; // HealOrb
    public Transform firePoint;
    public float orbSpeed = 22f;
    public int healAmount = 60;

    void Awake()
    {
        skillName = "회복의 빛";
        cooldown = 5f; // 필요시 조정
    }

    public override void Activate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject orb = PhotonNetwork.InstantiateRoomObject(healOrbPrefab.name, firePoint.position, firePoint.rotation);
        var orbComp = orb.GetComponent<HealOrb_Net>();
        if (orbComp) orbComp.healAmount = healAmount;

        var rb = orb.GetComponent<Rigidbody>();
        if (rb) rb.velocity = firePoint.forward * orbSpeed;
    }
}
