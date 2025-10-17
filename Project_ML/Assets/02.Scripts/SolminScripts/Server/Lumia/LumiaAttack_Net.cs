using UnityEngine;
using Photon.Pun;

public class LumiaAttack_Net : PlayerAttack_Net
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;         // 화살 프리팹
    public float minArrowSpeed = 10f;      // 최소 속도
    public float maxArrowSpeed = 50f;      // 최대 속도
    public float chargeTime = 1.5f;        // 최대 충전 시간
    private float currentCharge = 0f;

    [Header("References")]
    public Camera playerCamera;

    void Update()
    {
        if (!photonView.IsMine) return;

        // 충전 입력
        if (Input.GetButton("Fire1"))
            currentCharge = Mathf.Min(currentCharge + Time.deltaTime, chargeTime);

        // 발사
        if (Input.GetButtonUp("Fire1"))
        {
            RequestAttack(currentCharge);
            currentCharge = 0f;
        }
    }

    protected override void Attack(params object[] args)
    {
        float charge = (float)args[0];
        float percent = Mathf.Clamp01(charge / chargeTime);
        float speed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, percent);

        Vector3 dir = playerCamera.transform.forward;
        Vector3 spawnPos = firePoint.position;

        GameObject arrow = PhotonNetwork.Instantiate(arrowPrefab.name, spawnPos, Quaternion.LookRotation(dir));
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = dir * speed;
    }

    [PunRPC]
    protected override void Client_OnAttack(params object[] args)
    {
        // 화살 발사 효과, 소리 등 표시
    }
}
