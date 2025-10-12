using Photon.Pun;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthBarAutoBinder : MonoBehaviour, IPunInstantiateMagicCallback
{
    bool bound;

    void Start() { StartCoroutine(CoTryBind()); }
    public void OnPhotonInstantiate(PhotonMessageInfo info) { StartCoroutine(CoTryBind()); }

    IEnumerator CoTryBind()
    {
        if (bound) yield break;

        float t = 0f;
        while (t < 2f) // 씬 초기화 타이밍 대비, 최대 2초 재시도
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.BindOtherHPBar(gameObject);
                bound = true;
                yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }
    }
}
