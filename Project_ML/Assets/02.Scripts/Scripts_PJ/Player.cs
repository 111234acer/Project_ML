using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviourPun
{
    public int team = -1; // 0: Red, 1: Blue

    [PunRPC]
    void RequestTeamAssignment(int viewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 현재 팀 인원 집계
        int red = 0, blue = 0;
        foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            var p = obj.GetComponent<Player>();
            if (p == null) continue;
            if (p.team == 0) red++;
            else if (p.team == 1) blue++;
        }

        int assigned = (red <= blue) ? 0 : 1;

        // 해당 플레이어(오브너)에게만 결과 회신
        var targetView = PhotonView.Find(viewID);
        if (targetView != null)
            targetView.RPC(nameof(ConfirmTeamAssignment), targetView.Owner, assigned);
    }

    [PunRPC]
    void ConfirmTeamAssignment(int assignedTeam)
    {
        this.team = assignedTeam;

        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        Transform spawn = null;
        if (team == 0 && gm.redSpawnPoints.Length > 0)
            spawn = gm.redSpawnPoints[Random.Range(0, gm.redSpawnPoints.Length)];
        else if (team == 1 && gm.blueSpawnPoints.Length > 0)
            spawn = gm.blueSpawnPoints[Random.Range(0, gm.blueSpawnPoints.Length)];

        if (spawn != null)
            transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        Debug.Log($"플레이어 {name} 팀 배정 완료: {(team == 0 ? "Red" : "Blue")}");
    }

}