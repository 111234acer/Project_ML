using UnityEngine;
using Photon.Pun;
using System.Collections;

public class LumiaUlt_CP : PlayerSkillBase_CP
{
    public GameObject aoePrefab;          // 실제로 씬에 깔릴 이펙트/데미지 프리팹
    public GameObject aoeIndicatorPrefab; // 네가 만든 AOEIndicator 프리팹
    public float radius = 6f;
    public float damagePerSec = 25f;
    public float duration = 4f;
    public LayerMask groundMask;

    PhotonView pv;
    Animator anim;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
    }

    public override void Use()
    {
        if (!pv.IsMine) return;
        StartCoroutine(UltRoutine());
        StartCD();
    }

    IEnumerator UltRoutine()
    {
        // 1) 조준 표시
        GameObject indicator = Instantiate(aoeIndicatorPrefab);
        var a = indicator.GetComponent<AOEIndicator>();
        a.radius = radius;

        // 마우스 왼클릭으로 위치 확정
        while (!Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                indicator.transform.position = hit.point;
            }
            yield return null;
        }

        Vector3 castPos = indicator.transform.position;
        Destroy(indicator);

        // 2) 애니메이션
        anim?.SetTrigger("ultTrigger");
        yield return new WaitForSeconds(0.3f);

        // 3) AOE 생성 (데미지 포함)
        GameObject zone = Instantiate(aoePrefab, castPos, Quaternion.identity);
        zone.AddComponent<AOEDamageZone_CP>().Init(pv.ViewID, radius, damagePerSec, duration);

        // 4) 다른 클라에도 이펙트만
        pv.RPC(nameof(RPC_SpawnAoeFX), RpcTarget.Others, castPos);
    }

    [PunRPC]
    void RPC_SpawnAoeFX(Vector3 pos)
    {
        Instantiate(aoePrefab, pos, Quaternion.identity);
    }
}
