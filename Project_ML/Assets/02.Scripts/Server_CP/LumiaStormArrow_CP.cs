using UnityEngine;
using Photon.Pun;
using System.Collections;

public class LumiaStormArrow_CP : PlayerSkillBase_CP
{
    public Transform shootPoint;
    public GameObject arrowPrefab;
    public float arrowSpeed = 70f;
    public float damage = 25f;
    public int count = 3;
    public float interval = 0.12f;

    PhotonView pv;
    Animator anim;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
    }

    public override void Use()
    {
        if (!pv.IsMine) return;
        StartCoroutine(FireBurst());
        StartCD();
    }

    IEnumerator FireBurst()
    {
        anim?.SetTrigger("shootTrigger");
        pv.RPC(nameof(RPC_PlayFX), RpcTarget.Others);

        for (int i = 0; i < count; i++)
        {
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2));
            Vector3 dir = ray.direction;

            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.LookRotation(dir));
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.velocity = dir * arrowSpeed;

            arrow.AddComponent<ArrowProjectile_CP>().Init(pv.ViewID, damage);

            yield return new WaitForSeconds(interval);
        }
    }

    [PunRPC]
    void RPC_PlayFX()
    {
        anim?.SetTrigger("shootTrigger");
    }
}
