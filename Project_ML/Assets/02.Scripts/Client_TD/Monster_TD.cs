using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_TD : MonoBehaviour
{
    public float currentHP;
    public float maxHP = 100f;
    public float speed = 3f;
    public float stopDistance = 0.6f;
    public float attackDamage = 20f;
    public float attackRate = 2f;

    private float nextAttackTime = 0f;
    private float verticalSpeed;
    private float monsterRadius;

    public GameObject target;
    public Animator animator;

    private CharacterController cc;
    private Collider targetCol;

    private bool isDead = false;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        currentHP = maxHP;

        if (cc)
        {
            float xzScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            
            monsterRadius = Mathf.Max(0f, (cc.radius * xzScale) - cc.skinWidth * 0.5f);
        }
    }
    private void Start()
    {
        if (!target)
        {
            var tower = FindObjectOfType<TowerManager_TD>();
            if (tower)
            {
                target = tower.gameObject;
                targetCol = null;
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
    }

    public void MonsterMovement()
    {
        if (!targetCol)
        {
            targetCol = target.GetComponent<Collider>();
            if (!targetCol) return;
        }

        Vector3 pos = transform.position;
        Vector3 tpos = target.transform.position;
        Vector3 toTargetXZ = new Vector3(tpos.x - pos.x, 0f, tpos.z - pos.z);

        float dist2D = toTargetXZ.magnitude;
        Vector3 dir = dist2D > 0.1f ? (toTargetXZ / dist2D) : Vector3.zero;

        if (dir.sqrMagnitude > 0f)
        {
            transform.forward = dir;
        }

        float targetRadius = GetRadius(targetCol, target.transform.lossyScale);
        float surfaceDistance = dist2D - (monsterRadius + targetRadius);

        if (surfaceDistance > stopDistance)
        {
            Vector3 move = dir * speed;

            if (cc.isGrounded)
            {
                if (verticalSpeed < -2f) verticalSpeed = -2f;
            }
            else
            {
                verticalSpeed += Physics.gravity.y * Time.deltaTime;
            }

            move.y = verticalSpeed;

            cc.Move(move * Time.deltaTime);
        }
        else
        {

            if (!cc.isGrounded)
            {
                verticalSpeed += Physics.gravity.y * Time.deltaTime;
            }
            else if (verticalSpeed < -2f)
            {
                verticalSpeed = -2f;
            }

            cc.Move(new Vector3(0f, verticalSpeed, 0f) * Time.deltaTime);

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackRate;
            }

            return;
        }
    }

    public void Attack()
    {
        if (isDead) return;

        animator.SetTrigger("attackTrigger");

        TowerManager_TD tower = target.GetComponent<TowerManager_TD>();
        if (tower != null)
        {
            tower.TakeDamage(attackDamage, this);
        }
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
        Destroy(gameObject);
    }

    private static float GetRadius(Collider col, Vector3 lossyScale)
    {
        var cap = col as CapsuleCollider;
        if (!cap) return 0f;

        float planeScale = Mathf.Max(lossyScale.x, lossyScale.z); // Y축 정렬 → XZ 평면
        return cap.radius * planeScale;
    }
}