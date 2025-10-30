using UnityEngine;
using Photon.Pun;

// 네트워크 화살 (클라 생성, 서버 데미지 처리)
[DisallowMultipleComponent]
public class Arrow_Net : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Flight")]
    public float gravity = 18f;
    public float drag = 0.01f;
    public float lifeTime = 6f;
    public LayerMask hitMask = ~0;

    [Header("Damage")]
    public int baseDamage = 60;
    public float headshotMultiplier = 2f;
    public string headTag = "Head";

    private Vector3 velocity;
    private int ownerViewId;
    private float spawnTime;
    private bool hasHit;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        if (data != null && data.Length >= 4)
        {
            ownerViewId = (int)data[0];
            velocity = new Vector3((float)data[1], (float)data[2], (float)data[3]);
        }
    }

    void Start()
    {
        spawnTime = Time.time;
        transform.forward = velocity.normalized;
    }

    void Update()
    {
        if (hasHit) return;

        if (Time.time - spawnTime >= lifeTime)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        float dt = Time.deltaTime;
        velocity += Vector3.down * gravity * dt;
        velocity *= (1f - drag * dt);

        Vector3 step = velocity * dt;
        if (Physics.Raycast(transform.position, step.normalized, out RaycastHit hit, step.magnitude + 0.05f, hitMask))
        {
            OnHit(hit);
            return;
        }

        transform.position += step;
        transform.forward = Vector3.Lerp(transform.forward, velocity.normalized, 0.5f);
    }

    void OnHit(RaycastHit hit)
    {
        if (hasHit) return;
        hasHit = true;

        // 서버만 데미지 계산
        if (PhotonNetwork.IsMasterClient)
        {
            var hitPv = hit.collider.GetComponentInParent<PhotonView>();
            if (hitPv && hitPv.ViewID == ownerViewId)
            {
                PhotonNetwork.Destroy(gameObject);
                return;
            }

            int dmg = baseDamage;
            if (hit.collider.CompareTag(headTag))
                dmg = Mathf.RoundToInt(baseDamage * headshotMultiplier);

            var hp = hit.collider.GetComponentInParent<PlayerHealth_Server>();
            if (hp)
                hp.photonView.RPC(nameof(PlayerHealth_Server.Server_ApplyDamage), RpcTarget.MasterClient, dmg);
        }

        PhotonNetwork.Destroy(gameObject);
    }
}