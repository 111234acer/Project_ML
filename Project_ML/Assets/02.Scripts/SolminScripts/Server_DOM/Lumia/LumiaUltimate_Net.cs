using System.Collections;
using UnityEngine;
using Photon.Pun;

// [루미아 궁극기].
// AOE 이펙트는 서버에서 생성.
// 데미지는 서버만 계산 (AOEDamageZone_Net 내부에서 처리).
[DisallowMultipleComponent]
public class LumiaUltimate_Net : PlayerSkill_Net
{
    [Header("AOE Settings")]
    public GameObject indicatorPrefab;     // 조준 인디케이터
    public GameObject aoeEffectPrefab;     // AOE 피해 구역 Prefab (AOEDamageZone_Net 포함)
    public float range = 20f;              // 사거리 제한

    [Header("Damage Settings")]
    public float duration = 5f;            // AOE 유지 시간
    public float damagePerSecond = 30f;    // 초당 데미지

    [Header("References")]
    public Camera playerCamera;            // 플레이어 카메라
    public Transform player;               // 플레이어 본체 트랜스폼

    private GameObject indicatorInstance;
    private bool isTargeting = false;
    private AnimationHandler anim;

    void Awake()
    {
        skillName = "루미아 궁극기";
        cooldown = 15f;
        anim = GetComponentInChildren<AnimationHandler>();
    }

    public override void Activate()
    {
        if (!photonView.IsMine) return;
        if (isTargeting) return;

        // 스킬 사용 중 기본공격 잠금
        PlayerSkillManager_Net.SetSkillLock(true);
        StartCoroutine(TargetingRoutine());
    }


    // 조준 및 클릭 루프
    private IEnumerator TargetingRoutine()
    {
        isTargeting = true;
        indicatorInstance = Instantiate(indicatorPrefab);

        while (isTargeting)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                float dist = Vector3.Distance(player.position, hit.point);
                if (dist <= range)
                    indicatorInstance.transform.position = hit.point;
            }

            // 마우스 좌클릭 시 발동
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 spawnPos = indicatorInstance.transform.position;

                // 서버에 AOE 생성 요청 (서버만 피해 계산)
                photonView.RPC(nameof(Server_SpawnAOE), RpcTarget.MasterClient, spawnPos);

                // 인디케이터 제거 및 애니메이션 실행
                Destroy(indicatorInstance);
                isTargeting = false;
                photonView.RPC(nameof(Client_Anim_Skill3), RpcTarget.All);
            }

            // ESC로 취소
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(indicatorInstance);
                isTargeting = false;
            }

            yield return null;
        }

        // 조준 종료 → 잠금 해제 & 쿨다운 적용
        PlayerSkillManager_Net.SetSkillLock(false);
        EndSkill();
    }


    // 서버만 AOE 생성 (RoomObject)

    [PunRPC]
    void Server_SpawnAOE(Vector3 pos)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject aoe = PhotonNetwork.InstantiateRoomObject(aoeEffectPrefab.name, pos, Quaternion.identity);
        var zone = aoe.GetComponent<AOEDamageZone_Net>();
        if (zone != null)
            zone.Initialize(damagePerSecond, duration);
    }

    // 모든 클라에서 애니메이션 실행
    [PunRPC]
    void Client_Anim_Skill3()
    {
        anim?.Skill3Trigger();
    }
}
