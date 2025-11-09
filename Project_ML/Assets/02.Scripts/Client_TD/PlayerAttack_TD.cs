using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAttack_TD : MonoBehaviour
{
    [Header("공격 기본 설정")]
    [Tooltip("공격 간격 (초 단위)")]
    public float attackInterval;    
    [Tooltip("기본 공격력")]
    public float baseDamage;           
    [Tooltip("공격 사거리 (투사체 생존 시간 계산용)")]
    public float attackRange;
    [Tooltip("치명타 확률")]
    [Range(0f, 100f)] public float criticalChance = 5f; // 치명타 확률 5로 고정
    [Tooltip("치명타 데미지")]
    public float criticalMultiplier = 1.0f;             // 치명타 배율 (ex. 1배 = 100%)

    [Header("투사체 설정")]
    [Tooltip("투사체 프리팹 (활, 마법 등)")]
    public GameObject projectilePrefab;      // 발사체 프리팹
    [Tooltip("발사 위치 (활 시점, 손 위치 등)")]
    public Transform firePoint;              // 투사체 생성 위치
    [Tooltip("투사체 속도 (m/s)")]
    public float projectileSpeed = 40f;      // 투사체 속도

    AnimationHandler_TD ani;
    PlayerSFX_TD playerSFX;

    // 내부 전용
    private float attackTimer;               // 다음 공격까지 남은 시간 계산용
    private bool isAttacking;                // 현재 공격 중인지 여부
    public Camera mainCam;                  // 카메라 참조

    void Awake()
    {
        mainCam = Camera.main;
        ani = GetComponentInChildren<AnimationHandler_TD>();
        playerSFX = GetComponent<PlayerSFX_TD>();
    }

    void Update()
    {
        // 공격 타이머 갱신
        attackTimer += Time.deltaTime;

        // 일정 시간마다 자동 공격 실행
        if (attackTimer >= attackInterval && !isAttacking)
        {
            StartCoroutine(AutoFireRoutine());
        }
    }

    // 자동 공격 루프 (공격 간격마다 실행)
    private IEnumerator AutoFireRoutine()
    {
        isAttacking = true;
        attackTimer = 0f;

        yield return new WaitForSeconds(0.1f); // 발사 타이밍 보정용 짧은 딜레이

        FireProjectile(); // 실제 발사 수행

        isAttacking = false;
    }

    // 카메라 조준선 방향으로 투사체 발사
    private void FireProjectile()
    {
        if (!projectilePrefab || !firePoint) return;
        if (!mainCam) mainCam = Camera.main;

        // 카메라 중앙 방향으로 발사
        Vector3 dir = mainCam.transform.forward;
        Quaternion rot = Quaternion.LookRotation(dir);

        ani.AttackTrigger();
        playerSFX.PlayAttack();

        // 투사체 생성
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, rot);
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        // 물리 속도 부여
        if (rb)
            rb.useGravity = false;  // 포물선 true or 직선 false
            rb.velocity = dir * projectileSpeed;

        // 치명타 확률 계산
        float finalDamage = baseDamage;
        float rand = Random.Range(0f, 100f);

        if( rand <= criticalChance)
        {
            finalDamage *= criticalMultiplier;
            Debug.Log($"[CRITICAL HIT!] Damage = {finalDamage}");
        }

        // 데미지 전달
        Projectile_TD projectile = proj.GetComponent<Projectile_TD>();
        if (projectile)
            projectile.Init(finalDamage);

        // 일정 시간 후 자동 제거 (사거리 기반 계산)
        Destroy(proj, attackRange / projectileSpeed);
    }
}
