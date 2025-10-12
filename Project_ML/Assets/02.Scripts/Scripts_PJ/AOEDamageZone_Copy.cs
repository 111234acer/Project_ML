using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AOEDamageZone_Copy : MonoBehaviour, IPunInstantiateMagicCallback
{
    // 네트워크로 전달/동기화할 파라미터
    private float damagePerSecond;
    private float duration;
    public float radius = 5f;
    private int ownerTeam = -1;
    private int ownerViewID = -1;

    PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    // PhotonNetwork.Instantiate 시 전달된 InstantiationData 수신
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var data = info.photonView.InstantiationData;
        if (data != null)
        {
            // 순서: team, ownerViewID, radius, dps, duration
            if (data.Length >= 1) ownerTeam = (int)data[0];
            if (data.Length >= 2) ownerViewID = (int)data[1];
            if (data.Length >= 3) radius = (float)data[2];
            if (data.Length >= 4) damagePerSecond = (float)data[3];
            if (data.Length >= 5) duration = (float)data[4];
        }

        // 소유자만 틱 처리
        if (pv != null && pv.IsMine)
            StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        float timer = 0f;
        float tick = 1f;

        while (timer < duration)
        {
            // 반경 내 모든 콜라이더 검사(레이어 고정 의존 제거)
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, ~0, QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                var hp = hit.GetComponentInParent<PlayerHealth_Copy>();
                if (hp == null || hp.IsDead()) continue;

                var targetPv = hp.GetComponentInParent<PhotonView>();
                if (targetPv == null) continue;

                // 자기 자신/아군 무시
                if (ownerViewID != -1 && targetPv.ViewID == ownerViewID) continue;
                var teamComp = hp.GetComponentInParent<PlayerTeam>();
                if (teamComp != null && ownerTeam != -1 && teamComp.team == ownerTeam) continue;

                int dmg = Mathf.RoundToInt(damagePerSecond);
                targetPv.RPC("TakeDamage", RpcTarget.All, dmg);

                // 내 클라 기준, 맞은 적 HP바 표시
                if (pv != null && pv.IsMine)
                    OtherPlayerHealthBar.RegisterDamagedByLocal(targetPv.ViewID, 3f);
            }

            timer += tick;
            yield return new WaitForSeconds(tick);
        }

        // 수명 종료
        if (pv != null && pv.IsMine) PhotonNetwork.Destroy(gameObject);
        else Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
