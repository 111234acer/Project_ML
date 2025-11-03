using System.Collections;
using UnityEngine;
using Photon.Pun;

// [루미아 스킬1: 폭풍 화살]
// 3발 연속으로 빠르게 발사 (0.3초 간격)
// 화살 생성은 클라이언트, 데미지 계산은 서버.
// 애니메이션 트리거, HUD 쿨다운 모두 자동 연동.
[DisallowMultipleComponent]
public class LumiaSkill_StormArrow_Net : PlayerSkill_Net
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;      // Photon 등록된 화살 프리팹(Arrow_Net)
    public Transform firePoint;         // 화살 발사 위치
    public float arrowSpeed = 25f;      // 화살 속도
    public float fireInterval = 0.3f;   // 발사 간격 (초)
    public int arrowCount = 3;          // 총 3발
    public bool lockPrimaryDuringSkill = true; // 버스트 중 기본공격 잠금

    private AnimationHandler anim;

    void Awake()
    {
        skillName = "폭풍 화살";
        cooldown = 6f;
        anim = GetComponentInChildren<AnimationHandler>();
    }

    public override void Activate()
    {
        if (!photonView.IsMine) return;

        // 애니메이션 트리거 (전 클라)
        photonView.RPC(nameof(Client_Anim_Skill1), RpcTarget.All);

        // 3발 연속 발사 루틴 시작
        StartCoroutine(StormArrow());
    }

    IEnumerator StormArrow()
    {
        if (lockPrimaryDuringSkill)
            PlayerSkillManager_Net.SetSkillLock(true);

        for (int i = 0; i < arrowCount; i++)
        {
            FireOneArrow();
            yield return new WaitForSeconds(fireInterval);
        }

        if (lockPrimaryDuringSkill)
            PlayerSkillManager_Net.SetSkillLock(false);

        EndSkill(); // PlayerSkill_Net에서 쿨다운 자동 처리
    }

    void FireOneArrow()
    {
        if (arrowPrefab == null || firePoint == null) return;

        Vector3 shootDir = firePoint.forward;
        Vector3 v0 = shootDir * arrowSpeed;

        object[] data = new object[]
        {
            photonView.ViewID,
            v0.x, v0.y, v0.z
        };

        PhotonNetwork.Instantiate(
            arrowPrefab.name,
            firePoint.position,
            Quaternion.LookRotation(shootDir),
            0,
            data
        );
    }

    [PunRPC]
    void Client_Anim_Skill1()
    {
        anim?.Skill1Trigger();
    }
}
