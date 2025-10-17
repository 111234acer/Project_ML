using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaAttack_Copy : PlayerAttack
{
    PlayerSkillManager_Copy skillMgr;
    PlayerHealth_Copy health;
    PhotonView pv;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab;         // 화살 프리팹
    public float minArrowSpeed = 10f;      // 최소 속도
    public float maxArrowSpeed = 50f;      // 최대 속도
    public float chargeTime = 1.5f;        // 최대 충전 시간
    private float currentCharge = 0f;

    [Header("References")]
    public Camera playerCamera;            // 플레이어 카메라 (MainCamera)

    public static Action<float, bool> OnChargeUpdate;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        skillMgr = GetComponentInParent<PlayerSkillManager_Copy>();
        health = GetComponent<PlayerHealth_Copy>();
    }

    private void Update()
    {
        if (pv != null && PhotonNetwork.InRoom && !pv.IsMine) return;

        if (health != null && health.isDead)
        {
            if (currentCharge > 0f && OnChargeUpdate != null) OnChargeUpdate(0f, false);
            return;
        }

        if (pv != null && pv.IsMine && skillMgr != null && Time.time < skillMgr.suppressFire1Until)
        {
            if (currentCharge > 0f && OnChargeUpdate != null) OnChargeUpdate(0f, false);
            currentCharge = 0f;
            return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (!CanAttack())
            {
                if (OnChargeUpdate != null) OnChargeUpdate(0f, false);
                return;
            }
            currentCharge = 0f;
            if (OnChargeUpdate != null) OnChargeUpdate(0f, true);
        }

        // 공격 버튼 누르고 있으면 충전
        if (Input.GetButton("Fire1"))
        {
            if (!CanAttack())
            {
                if (OnChargeUpdate != null) OnChargeUpdate(0f, false);
                return;
            }

            currentCharge += Time.deltaTime;
            if (currentCharge > chargeTime) currentCharge = chargeTime;

            float percent = currentCharge / chargeTime;
            if (OnChargeUpdate != null) OnChargeUpdate(percent, true);
        }

        // 버튼 떼면 발사
        if (Input.GetButtonUp("Fire1"))
        {
            if (CanAttack())
            {
                Attack();
                UpdateFireTime();
            }

            if (OnChargeUpdate != null) OnChargeUpdate(0f, false);
            currentCharge = 0f;
        }
    }

    public override void Attack()
    {
        if (health != null && health.isDead) return;

        if (arrowPrefab == null || playerCamera == null) return;

        // 발사 방향 = 카메라 중앙
        Vector3 shootDir = playerCamera.transform.forward;

        // 충전 비율 (0 ~ 1)
        float chargePercent = currentCharge / chargeTime;

        // 속도 계산
        float arrowSpeed = Mathf.Lerp(minArrowSpeed, maxArrowSpeed, chargePercent);

        // 화살 생성 (카메라 앞에서)
        Vector3 spawnPos = playerCamera.transform.position + shootDir * 0.5f;

        int ownerTeam = -1;
        var teamA = GetComponentInParent<PlayerTeam>();
        if (teamA != null) ownerTeam = teamA.team;        
        int ownerViewID = (pv != null) ? pv.ViewID : -1;

        object[] data = new object[] { ownerTeam, ownerViewID };
        var arrow = PhotonNetwork.Instantiate(arrowPrefab.name, spawnPos, Quaternion.LookRotation(shootDir), 0, data);

        var rb = arrow.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = shootDir * arrowSpeed;

        currentCharge = 0f;
    }
}
