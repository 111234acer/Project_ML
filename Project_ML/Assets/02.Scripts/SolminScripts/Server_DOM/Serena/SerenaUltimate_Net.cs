using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SeraUltimate_Net : PlayerSkill_Net
{
    [Header("AOE Heal")]
    public GameObject indicatorPrefab;   // 로컬 전용
    public GameObject aoeHealZonePrefab; // AOE_HealZone
    public float range = 20f;

    [Header("Heal Values")]
    public int healPerSecond = 30;
    public float duration = 5f;

    [Header("Refs")]
    public Camera playerCamera;
    public Transform player;

    GameObject indicatorInstance;
    bool isTargeting;

    void Awake()
    {
        skillName = "성역";
        cooldown = 15f;
    }

    public override void Activate()
    {
        if (!photonView.IsMine || isTargeting) return;
        StartCoroutine(Targeting());
    }

    IEnumerator Targeting()
    {
        isTargeting = true;
        indicatorInstance = Instantiate(indicatorPrefab);

        while (isTargeting)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (Vector3.Distance(player.position, hit.point) <= range)
                    indicatorInstance.transform.position = hit.point;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = indicatorInstance.transform.position;
                photonView.RPC(nameof(Server_SpawnAOEHeal), RpcTarget.MasterClient, pos, healPerSecond, duration);
                Destroy(indicatorInstance);
                isTargeting = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(indicatorInstance);
                isTargeting = false;
            }
            yield return null;
        }
    }

    [PunRPC]
    void Server_SpawnAOEHeal(Vector3 pos, int hps, float dur)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GameObject aoe = PhotonNetwork.InstantiateRoomObject(aoeHealZonePrefab.name, pos, Quaternion.identity);
        aoe.GetComponent<AOEHealZone_Net>()?.Initialize(hps, dur);
    }
}
