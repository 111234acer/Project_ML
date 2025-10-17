using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiaArrow_Copy : MonoBehaviour, IPunInstantiateMagicCallback
{
    [Header("Arrow Settings")]
    public int damage = 30;                     // 공격력
    public float lifeTime = 5f;                 // 자동 제거 시간   

    int ownerTeam = -1;
    int ownerViewID = -1;

    PhotonView pv;
    Collider[] myCols;

    bool handled = false;
    void Awake()
    {
        pv = GetComponent<PhotonView>();        
        myCols = GetComponentsInChildren<Collider>(true);
    }

    private void Start()
    {
        if (pv != null && pv.IsMine) StartCoroutine(CoLifetime());
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var data = info.photonView.InstantiationData;
        if (data != null && data.Length >= 2)
        {
            ownerTeam = (int)data[0];
            ownerViewID = (int)data[1];
        }

        StartCoroutine(CoIgnoreOwnerOnceReady());
    }

    IEnumerator CoLifetime()
    {
        yield return new WaitForSeconds(lifeTime);
        if (pv != null && pv.IsMine && gameObject != null)
            PhotonNetwork.Destroy(gameObject);
    }

    IEnumerator CoIgnoreOwnerOnceReady()
    {
        // 내 콜라이더 준비 대기
        while (myCols == null || myCols.Length == 0)
        {
            myCols = GetComponentsInChildren<Collider>(true);
            yield return null;
        }

        // 오너 찾기
        PhotonView ownerPv = null;
        float timer = 0f;
        while (ownerPv == null && timer < 1f) // 1초 한정 재시도
        {
            ownerPv = PhotonView.Find(ownerViewID);
            timer += Time.deltaTime;
            yield return null;
        }
        if (ownerPv == null) yield break;

        var ownerCols = ownerPv.GetComponentsInChildren<Collider>(true);
        foreach (var mine in myCols)
        {
            if (mine == null) continue;
            foreach (var oc in ownerCols)
            {
                if (oc == null) continue;
                Physics.IgnoreCollision(mine, oc, true);
            }
        }
    }
    void OnCollisionEnter(Collision c) { TryHit(c.collider); }
    void OnTriggerEnter(Collider other) { TryHit(other); }

    void TryHit(Collider hit)
    {
        if (!(pv != null && pv.IsMine)) return;
        if (handled) return; handled = true;
                
        // 자기 자신 무시
        var targetPv = hit.GetComponentInParent<PhotonView>();
        if (targetPv != null && targetPv.ViewID == ownerViewID) return;

        // 아군 무시
        var teamComp = hit.GetComponentInParent<PlayerTeam>();
        if (teamComp != null && ownerTeam != -1 && teamComp.team == ownerTeam) return;

        // 플레이어 아닌 경우(벽/지형 등) → 화살만 삭제
        var targetHp = hit.GetComponentInParent<PlayerHealth_Copy>();
        if (targetHp == null)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        // 데미지 계산 (헤드 2배)
        int finalDamage = damage;
        if (hit.CompareTag("Head")) finalDamage = damage * 2;

        // RPC로 데미지 전파
        if (targetPv != null)
        {
            targetPv.RPC("TakeDamage", RpcTarget.All, finalDamage);
            
            OtherPlayerHealthBar.RegisterDamagedByLocal(targetPv.ViewID, 3f);
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
