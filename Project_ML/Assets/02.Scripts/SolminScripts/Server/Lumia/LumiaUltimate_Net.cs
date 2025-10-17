using System.Collections;
using UnityEngine;
using Photon.Pun;

public class LumiaUltimate_Net : PlayerSkill_Net
{
    [Header("AOE Settings")]
    public GameObject indicatorPrefab;
    public GameObject aoeEffectPrefab;
    public float range = 20f;

    [Header("Damage Settings")]
    public float duration = 5f;
    public float damagePerSecond = 30f;

    [Header("References")]
    public Camera playerCamera;
    public Transform player;

    private GameObject indicatorInstance;
    private bool isTargeting = false;

    private void Awake()
    {
        skillName = "·ç¹Ì¾Æ ±Ã±Ø±â";
        cooldown = 15f;
    }

    public override void Activate()
    {
        if (!photonView.IsMine) return;
        if (isTargeting) return;
        StartCoroutine(TargetingRoutine());
    }

    private IEnumerator TargetingRoutine()
    {
        isTargeting = true;
        indicatorInstance = Instantiate(indicatorPrefab);

        while (isTargeting)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                float distance = Vector3.Distance(player.position, hit.point);
                if (distance <= range)
                    indicatorInstance.transform.position = hit.point;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 spawnPos = indicatorInstance.transform.position;
                photonView.RPC(nameof(Server_SpawnAOE), RpcTarget.MasterClient, spawnPos);
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
    void Server_SpawnAOE(Vector3 pos)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject aoe = PhotonNetwork.InstantiateRoomObject(aoeEffectPrefab.name, pos, Quaternion.identity);
        var zone = aoe.GetComponent<AOEDamageZone_Net>();
        if (zone != null)
            zone.Initialize(damagePerSecond, duration);
    }
}
