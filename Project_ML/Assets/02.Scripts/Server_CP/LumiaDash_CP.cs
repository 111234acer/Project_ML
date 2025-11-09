using UnityEngine;
using Photon.Pun;
using System.Collections;

public class LumiaDash_CP : PlayerSkillBase_CP
{
    public float distance = 5f;
    public float dashTime = 0.15f;

    PhotonView pv;
    CharacterController cc;
    PlayerInput_CP input;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        cc = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput_CP>();
    }

    public override void Use()
    {
        if (!pv.IsMine) return;
        StartCoroutine(DashRoutine());
        StartCD();
    }

    IEnumerator DashRoutine()
    {
        float elapsed = 0f;

        // WASD 방향
        Vector3 moveDir = new Vector3(input.move.x, 0f, input.move.y);
        if (moveDir.sqrMagnitude < 0.1f)
            moveDir = transform.forward; // 입력 없으면 바라보는 방향

        moveDir.Normalize();

        while (elapsed < dashTime)
        {
            cc.Move(moveDir * (distance / dashTime) * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
