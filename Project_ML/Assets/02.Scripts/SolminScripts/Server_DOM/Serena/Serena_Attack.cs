using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class SeraAttack_Net : MonoBehaviourPun
{
    [Header("Projectile")]
    public GameObject damageBoltPrefab; // DamageBolt
    public Transform firePoint;
    public float boltSpeed = 30f;

    [Header("Fire")]
    public float fireRate = 1f; // ÃÊ´ç 1¹ß
    float nextFireTime;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);
            photonView.RPC(nameof(Server_FireBolt), RpcTarget.MasterClient, firePoint.position, firePoint.rotation);
        }
    }

    [PunRPC]
    void Server_FireBolt(Vector3 pos, Quaternion rot, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject bolt = PhotonNetwork.InstantiateRoomObject(damageBoltPrefab.name, pos, rot);
        var rb = bolt.GetComponent<Rigidbody>();
        if (rb) rb.velocity = rot * Vector3.forward * boltSpeed;
    }
}
