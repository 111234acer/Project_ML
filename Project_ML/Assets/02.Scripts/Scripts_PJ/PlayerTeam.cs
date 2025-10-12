using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PUNPlayer = Photon.Realtime.Player;

public class PlayerTeam : MonoBehaviour
{
    public int team = -1; // 0: Red, 1: Blue

    PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public void SetTeamNetworked(int t)
    {
        if (pv != null)
            pv.RPC("RPC_SetTeam", RpcTarget.AllBuffered, t);
    }

    [PunRPC]
    void RPC_SetTeam(int t)
    {
        team = t;
        // 팀에 따른 외형/레이어 세팅이 있으면 여기서 같이 반영(기존 로직 호출만)
        // e.g., ApplyTeamVisual(team);
    }

    public void AssignTeam()
    {
        if (pv != null && !pv.IsMine) return;

        // 1) 커스텀 프로퍼티 우선 (로비/룸에서 정해둔 값)
        int decided;
        if (TryGetTeamProp(PhotonNetwork.LocalPlayer, out decided))
        {
            SetTeamNetworked(decided);
            return;
        }

        // 2) 네트워크 플레이어 리스트로 균형 배정
        int red = 0, blue = 0;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            int t;
            if (TryGetTeamProp(p, out t))
            {
                if (t == 0) red++;
                else if (t == 1) blue++;
            }
        }
        decided = (red <= blue) ? 0 : 1;

        // 3) (마지막 폴백) 씬 내 오브젝트 집계 네트워크 정보가 전혀 없을 때만
        if (red == 0 && blue == 0)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            int sRed = 0, sBlue = 0;
            for (int i = 0; i < players.Length; i++)
            {
                var pl = players[i].GetComponent<PlayerTeam>();
                if (pl == null) continue;
                if (pl.team == 0) sRed++;
                else if (pl.team == 1) sBlue++;
            }
            decided = (sRed <= sBlue) ? 0 : 1;
        }

        SetTeamNetworked(decided);
    }
    bool TryGetTeamProp(PUNPlayer p, out int t)
    {
        t = -1;
        if (p == null || p.CustomProperties == null) return false;

        object v;
        if (p.CustomProperties.TryGetValue("MyTeam", out v) ||
            p.CustomProperties.TryGetValue("Team", out v) ||
            p.CustomProperties.TryGetValue("team", out v))
        {
            if (v is int i) { t = Mathf.Clamp(i, 0, 1); return true; }
            if (v is byte b) { t = Mathf.Clamp((int)b, 0, 1); return true; }
        }
        return false;
    }
}