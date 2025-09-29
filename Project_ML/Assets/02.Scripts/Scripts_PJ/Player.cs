using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int team = -1; // 0: Red, 1: Blue

    public void AssignTeam()
    {
        // 현재 씬에 있는 Player 오브젝트 집계
        int red = 0, blue = 0;
        var players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i].GetComponent<Player>();
            if (p == null) continue;
            if (p.team == 0) red++;
            else if (p.team == 1) blue++;
        }

        // 더 적은 쪽 팀에 배정
        team = (red <= blue) ? 0 : 1;
    }
}