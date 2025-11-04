using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_TD : MonoBehaviour
{
    public float currentHP;
    public float maxHP = 100f;
    public float speed = 3f;
    public float stopDistance = 1f;
    public float attackDamage = 20f;
    public float attackRate = 2f;
    [HideInInspector] public float baseMaxHP;
    [HideInInspector] public float baseAttackDamage;

    private float nextAttackTime = 0f;
    private float monsterRadius;

    private float currentSpeed;

    [HideInInspector] public GameObject originalPrefab;
    public GameObject target;
    public Animator animator;

    private MonsterSpawner_TD _spawner;
    private Rigidbody rb;
    private CapsuleCollider cap;
    private Collider targetCol;

    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cap = GetComponent<CapsuleCollider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        currentHP = maxHP;

        baseMaxHP = maxHP;
        baseAttackDamage = attackDamage;

        float xzScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        monsterRadius = Mathf.Max(0f, (cap.radius * xzScale) - 0.01f);
    }
    private void Start()
    {
        if (!target)
        {
            var tower = FindObjectOfType<TowerManager_TD>();
            if (tower)
            {
                target = tower.gameObject;
            }
            else
            {
                Debug.LogWarning("[Monster] 타워를 찾지 못했습니다.");
            }
        }
    }

    private void Update()
    {
        if (!target || isDead) return;

        MonsterMovement();
        animator.SetFloat("speed", currentSpeed);
    }

    public void MonsterMovement()
    {
        if (!targetCol)
        {
            targetCol = target.GetComponent<Collider>();
            if (!targetCol) return;
        }

        Vector3 pos = transform.position;
        Vector3 tpos = targetCol.ClosestPoint(pos);
        Vector3 toTarget = tpos - pos;

        float dist = toTarget.magnitude;

        if (dist > stopDistance)
        {
            Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z).normalized;

            if (flatDir.sqrMagnitude > 0.1f)
            {
                transform.forward = flatDir;
            }

            Vector3 vel = flatDir * speed;
            vel.y = rb.velocity.y;
            rb.velocity = vel;

            currentSpeed = 1;
        }
        else
        {
            currentSpeed = 0;

            Vector3 vel = rb.velocity;

            vel.x = 0f;
            vel.z = 0f;
            rb.velocity = vel;

            Attack();
        }
    }

    public void Attack()
    {
        if (isDead) return;
        if (Time.time < nextAttackTime) return;

        TowerManager_TD tower = target.GetComponent<TowerManager_TD>();
        if (tower != null)
        {
            //tower.TakeDamage(attackDamage,this); 이거 수정해야함
        }

        animator.SetTrigger("attackTrigger");

        nextAttackTime = Time.time + attackRate;

    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= amount;

        Debug.Log($"[Monster] {amount} 데미지 받음, 현재 체력 : {currentHP:F1}");

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            StartCoroutine(DeadAnim());
        }
    }

    IEnumerator DeadAnim()
    {
        if (isDead) yield break;
        isDead = true;

        animator.SetTrigger("deadTrigger");
        yield return new WaitForSeconds(2f);

        if (_spawner != null && originalPrefab != null)
        {
            _spawner.ReturnToPool(originalPrefab, gameObject);
        }
    }

    public void OnSpawnedFromPool(MonsterSpawner_TD spawner)
    {
        _spawner = spawner;
    }

    // 스폰될 때마다 상태 초기화
    public void ResetMonster()
    {
        currentHP = maxHP;
        isDead = false;
        nextAttackTime = 0f;
        currentSpeed = 0f;
        targetCol = null;

        // 리지드바디 멈춰두기
        if (rb)
            rb.velocity = Vector3.zero;

        if (animator)
        {
            animator.ResetTrigger("deadTrigger");
            animator.SetFloat("speed", 0f);
        }
    }

    public void ApplyWaveBuff(int addHP, int addATK)
    {
        maxHP = baseMaxHP + addHP;
        attackDamage = baseAttackDamage + addATK;
        currentHP = maxHP; // 새로 나온 몬스터니까 풀피로
    }
}