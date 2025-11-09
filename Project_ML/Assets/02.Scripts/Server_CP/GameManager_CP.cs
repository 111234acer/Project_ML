using UnityEngine;
using Photon.Pun;
using System.Collections;

public class GameManager_CP : MonoBehaviourPun
{
    public static GameManager_CP Instance;

    public Transform[] team1Spawns;
    public Transform[] team2Spawns;
    public float winProgress = 100f;

    void Awake()
    {
        Instance = this;
    }

    public void RequestRespawn_CP(PlayerHealth_CP player)
    {
        StartCoroutine(RespawnRoutine(player));
    }

    IEnumerator RespawnRoutine(PlayerHealth_CP player)
    {
        yield return new WaitForSeconds(3f);

        var team = player.GetComponent<PlayerTeam_CP>();
        Vector3 spawnPos = Vector3.zero;

        if (team != null)
        {
            Transform[] arr = team.teamNumber == 1 ? team1Spawns : team2Spawns;
            if (arr != null && arr.Length > 0)
            {
                spawnPos = arr[Random.Range(0, arr.Length)].position;
            }
        }

        player.ReviveAt(spawnPos);
    }

    public void OnCaptureProgress_CP(float t1, float t2)
    {
        if (t1 >= winProgress)
        {
            OnTeamWin(1);
        }
        else if (t2 >= winProgress)
        {
            OnTeamWin(2);
        }
    }

    void OnTeamWin(int team)
    {
        Debug.Log($"Team {team} Win!");
        // TODO: °á°ú UI ¶ç¿ì±â, ¾À Á¾·á µî
    }
}
