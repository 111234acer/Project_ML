using System.Collections;
using System.Linq;
using UnityEngine;
using Photon.Pun;


// [AOE 피해 존]
// 서버(MasterClient)만 주기적으로 피해 계산.
[DisallowMultipleComponent]
public class AOEDamageZone_Net : MonoBehaviourPun
{
    [Header("AOE Settings")]
    public float damagePerSecond = 30f;
    public float duration = 5f;
    public float radius = 5f;
    public LayerMask hitMask;

    float elapsed;

    public void Initialize(float dps, float dur)
    {
        damagePerSecond = dps;
        duration = dur;

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(CoServerDamage());
        else
            StartCoroutine(CoDestroyAfter(duration)); // 클라에선 단순 시각효과 유지
    }

    IEnumerator CoServerDamage()
    {
        float tick = 1f; // 1초당 데미지 계산
        elapsed = 0f;

        while (elapsed < duration)
        {
            // 서버에서만 피해 계산
            if (PhotonNetwork.IsMasterClient)
            {
                var players = FindObjectsOfType<PlayerHealth_Server>()
                    .Where(p => !p.isDead && Vector3.Distance(p.transform.position, transform.position) <= radius);

                foreach (var hp in players)
                {
                    hp.photonView.RPC(
                        nameof(PlayerHealth_Server.Server_ApplyDamage),
                        RpcTarget.MasterClient,
                        Mathf.RoundToInt(damagePerSecond)
                    );
                }
            }

            yield return new WaitForSeconds(tick);
            elapsed += tick;
        }

        PhotonNetwork.Destroy(gameObject);
    }

    IEnumerator CoDestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.4f, 1f, 0.35f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
