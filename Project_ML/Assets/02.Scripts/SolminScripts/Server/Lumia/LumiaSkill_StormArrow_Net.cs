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

    private void Awake()
    {
        skillName = "ÆøÇ³ È­»ì";
        cooldown = 6f;
    }

    public override void Activate()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(FireRoutine());
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
}
