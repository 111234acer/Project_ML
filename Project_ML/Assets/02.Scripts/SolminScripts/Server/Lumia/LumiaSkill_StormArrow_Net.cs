using System.Collections;
using UnityEngine;
using Photon.Pun;

public class LumiaSkill_StormArrow_Net : PlayerSkill_Net
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowSpeed = 25f;
    public float fireInterval = 0.3f;
    public int arrowCount = 3;

    private AnimationHandler animationHandler;

    private void Awake()
    {
        skillName = "ÆøÇ³ È­»ì";
        cooldown = 6f;

        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    public override void Activate()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(FireRoutine());

        photonView.RPC("Client_Anim_Skill1", RpcTarget.All);
    }

    private IEnumerator FireRoutine()
    {
        for (int i = 0; i < arrowCount; i++)
        {
            GameObject arrow = PhotonNetwork.InstantiateRoomObject(arrowPrefab.name, firePoint.position, firePoint.rotation);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = firePoint.forward * arrowSpeed;

            yield return new WaitForSeconds(fireInterval);
        }
    }

    [PunRPC] 
    void Client_Anim_Skill1() 
    { 
        animationHandler?.Skill1Trigger();
    }
}
