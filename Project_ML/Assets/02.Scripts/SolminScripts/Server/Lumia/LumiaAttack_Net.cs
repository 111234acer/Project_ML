using UnityEngine;
using Photon.Pun;

// 클라이언트가 화살 생성.
// 서버만 데미지 계산.
// PlayerAttack_Net을 상속해 쿨다운 관리.
[DisallowMultipleComponent]
public class LumiaAttack_Net : PlayerAttack_Net
{
    [Header("References")]
    public Transform firePoint;      // 화살이 발사되는 위치
    public GameObject arrowPrefab;   // Photon 등록된 화살 프리팹

    [Header("Charge Settings")]
    public float minCharge = 0.1f;
    public float maxCharge = 1.5f;
    public AnimationCurve powerCurve = AnimationCurve.EaseInOut(0, 0.4f, 1, 1f);
    public float muzzleSpeed = 50f;

    private bool isCharging;
    private float chargeTime;

    void Update()
    {
        if (!photonView.IsMine) return;
        if (PlayerSkillManager_Net.IsUsingAnySkill) return; // 스킬 중엔 공격 잠금

        bool down = Input.GetMouseButtonDown(0);
        bool hold = Input.GetMouseButton(0);
        bool up = Input.GetMouseButtonUp(0);

        if (down && CanAttack())
        {
            isCharging = true;
            chargeTime = 0f;
            GetComponentInChildren<AnimationHandler>()?.ChargeStartTrigger();
        }

        if (isCharging && hold)
        {
            chargeTime = Mathf.Min(maxCharge, chargeTime + Time.deltaTime);
            CrosshairChargeUI.OnChargeUpdate?.Invoke(chargeTime / maxCharge, true);
        }

        if (isCharging && up)
        {
            PerformAttack(); // PlayerAttack_Net의 추상 함수 호출
        }
    }


    // 기본 공격 실행
    public override void PerformAttack()
    {
        if (!CanAttack()) return;
        ResetCooldown();
        isCharging = false;

        float t = Mathf.InverseLerp(0f, maxCharge, Mathf.Max(minCharge, chargeTime));
        float power = powerCurve.Evaluate(t);

        Vector3 shootDir = firePoint.forward;
        Vector3 shootVel = shootDir * (muzzleSpeed * Mathf.Lerp(0.5f, 1f, power));
        Vector3 spawnPos = firePoint.position;

        // 클라이언트가 직접 화살 생성 (전 클라 동기화)
        object[] data = new object[]
        {
            photonView.ViewID,
            shootVel.x, shootVel.y, shootVel.z
        };

        PhotonNetwork.Instantiate(arrowPrefab.name, spawnPos, Quaternion.LookRotation(shootDir), 0, data);

        // 발사 애니메이션 트리거
        GetComponentInChildren<AnimationHandler>()?.ShootTrigger();

        // 차지 UI 리셋
        CrosshairChargeUI.OnChargeUpdate?.Invoke(0f, false);
    }
}
