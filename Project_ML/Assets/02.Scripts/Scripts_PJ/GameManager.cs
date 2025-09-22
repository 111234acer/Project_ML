using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Spawn Points")]
    public Transform[] redSpawnPoints;
    public Transform[] blueSpawnPoints;

    [Header("Player Prefab")]
    public string playerPrefabName = "PlayerPrefab"; // Resources/PlayerPrefab

    void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    IEnumerator SpawnWhenReady()
    {
        // 방에 붙어 있을 때까지 대기 (로비 → 인게임 씬 이후에도 true 상태)
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom);

        // 이미 스폰된 적 있으면 중복 생성 방지
        if (PhotonNetwork.LocalPlayer.TagObject != null) yield break;

        // 임시 스폰 위치 (팀 배정 전)
        Transform spawn = (redSpawnPoints != null && redSpawnPoints.Length > 0)
            ? redSpawnPoints[0]
            : transform;

        var playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawn.position, spawn.rotation);

        // 중복 스폰 방지용으로 내 플레이어 보관
        PhotonNetwork.LocalPlayer.TagObject = playerObj;

        // 팀 배정 요청 (PlayerPrefab의 PhotonView로 RPC를 쏘는 게 가장 안전)
        var pv = playerObj.GetComponent<PhotonView>();
        pv.RPC("RequestTeamAssignment", RpcTarget.MasterClient, pv.ViewID);
    }
}