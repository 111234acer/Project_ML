using UnityEngine;
using Photon.Pun;

public class LumiaAttack_CP : MonoBehaviourPun
{
    public Transform shootPoint;
    public GameObject arrowPrefab;
    public float maxChargeTime = 1.5f;
    public float minForce = 15f;
    public float maxForce = 45f;
    public float baseDamage = 20f;
    public float maxDamage = 60f;
    public float gravity = 9.81f;

    PlayerInput_CP input;
    Animator anim;

    bool isCharging = false;
    float chargeTime = 0f;

    void Awake()
    {
        input = GetComponent<PlayerInput_CP>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 차지 시작
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTime = 0f;
            anim?.SetBool("isCharging", true);
        }

        // 차지 중
        if (isCharging)
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
        }

        // 발사
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            FireChargedArrow();
        }
    }

    void FireChargedArrow()
    {
        isCharging = false;
        anim?.SetBool("isCharging", false);
        anim?.SetTrigger("shootTrigger");

        float t = chargeTime / maxChargeTime;
        float force = Mathf.Lerp(minForce, maxForce, t);
        float dmg = Mathf.Lerp(baseDamage, maxDamage, t);

        // 화면 중앙 조준점으로 레이 쏘기
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));
        Vector3 targetPoint = ray.origin + ray.direction * 50f;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            targetPoint = hit.point;

        // 포물선 방향 계산
        Vector3 dir = GetParabolicDirection(shootPoint.position, targetPoint, force);

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.LookRotation(dir));
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.velocity = dir * force;

        arrow.AddComponent<ArrowProjectile_CP>().Init(photonView.ViewID, dmg);
    }

    // 단순 포물선 방향 계산
    Vector3 GetParabolicDirection(Vector3 start, Vector3 target, float speed)
    {
        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float y = toTarget.y;
        float xz = toTargetXZ.magnitude;

        float v2 = speed * speed;
        float g = gravity;
        float inside = v2 * v2 - g * (g * xz * xz + 2 * y * v2);

        float angle;
        if (inside <= 0f)
        {
            angle = 45f * Mathf.Deg2Rad;
        }
        else
        {
            float root = Mathf.Sqrt(inside);
            angle = Mathf.Atan((v2 - root) / (g * xz));
        }

        Vector3 dir = toTargetXZ.normalized;
        // x-축 회전이 아니라 위로 기울이려면 Vector3.up 기준으로 올려도 됨
        dir.y = Mathf.Tan(angle);
        return dir.normalized;
    }
}
