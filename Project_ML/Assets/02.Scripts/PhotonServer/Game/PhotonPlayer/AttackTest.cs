using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTest : PlayerAttack
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;         // 화살 프리팹
    public float minArrowSpeed = 10f;      // 최소 속도
    public float maxArrowSpeed = 50f;      // 최대 속도
    public float chargeTime = 1.5f;        // 최대 충전 시간
    private float currentCharge = 0f;

    [Header("References")]
    public Camera playerCamera;            // 플레이어 카메라 (MainCamera)

    PhotonView pv;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!pv.IsMine)
            return;

        // 공격 버튼 누르고 있으면 충전
        if (Input.GetButton("Fire1"))
        {
            currentCharge += Time.deltaTime;
            currentCharge = Mathf.Min(currentCharge, chargeTime);
        }

        // 버튼 떼면 발사
        if (Input.GetButtonUp("Fire1") && CanAttack())
        {
            // 방법1: 화살 포톤 네트워크 생성 (OnPhotonSerializeView를 활용한 위치 동기화)
            //Attack();

            // 방법2: 화살 포톤 네트워크 생성 (Photon Transform View를 활용한 위치 동기화)
            //Attack();

            // 방법3 : 화살 로컬로 생성 |||| (카메라 위치,회전 동기화 필요) -> (수정)(카메라 동기화 안하고 생성 위치와 방향을 RPC의 매개변수로 쏘는 방식)
            // 충전 비율 (0 ~ 1)
            float chargePercent = currentCharge / chargeTime;

            Vector3 shootDir = playerCamera.transform.forward;
            Vector3 spawnPos = playerCamera.transform.position + shootDir * 0.5f;
            Attack2(chargePercent, spawnPos, shootDir);
            pv.RPC(nameof(Attack2), RpcTarget.Others, chargePercent, spawnPos, shootDir);

            UpdateFireTime();
        }
    }

    // 화살에 포톤뷰 다는 방식
    public override void Attack() // 방법1,2
    {
        if (arrowPrefab == null || playerCamera == null) return;

        // 발사 방향 = 카메라 중앙
        Vector3 shootDir = playerCamera.transform.forward;

        // 충전 비율 (0 ~ 1)
        float chargePercent = currentCharge / chargeTime;

        // 속도 계산
        float arrowSpeed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, chargePercent);

        // 화살 생성 (카메라 앞에서)
        Vector3 spawnPos = playerCamera.transform.position + shootDir * 0.5f;
        //GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(shootDir));
        GameObject arrow = PhotonNetwork.Instantiate("PhotonArrow", spawnPos, Quaternion.identity, 0);
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = shootDir * arrowSpeed;
        }

        // 초기화
        currentCharge = 0f;
    }

    [PunRPC]
    public void Attack2(float chargePercent, Vector3 pos, Vector3 dir) // 방법3
    {
        if (arrowPrefab == null || playerCamera == null) return;

        // 발사 방향 = 카메라 중앙
        //Vector3 shootDir = playerCamera.transform.forward;
        Vector3 shootDir = dir;

        // 속도 계산
        float arrowSpeed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, chargePercent);

        // 화살 생성 (카메라 앞에서)
        //Vector3 spawnPos = playerCamera.transform.position + shootDir * 0.5f;
        //GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(shootDir));
        GameObject arrow = Instantiate(arrowPrefab, pos, Quaternion.LookRotation(shootDir));
        arrow.GetComponent<PhotonArrow>().ownerId = pv.Owner.ActorNumber;
        if(pv.Owner.CustomProperties.ContainsKey("MyTeam"))
        {
            arrow.GetComponent<PhotonArrow>().teamOfOwner = (string)pv.Owner.CustomProperties["MyTeam"];
        }
        //GameObject arrow = PhotonNetwork.Instantiate("PhotonArrow", spawnPos, Quaternion.identity, 0);
        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = shootDir * arrowSpeed;
        }

        // 초기화
        currentCharge = 0f;
    }
}
