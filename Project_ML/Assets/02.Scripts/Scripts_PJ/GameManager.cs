using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Spawn Areas")]
    public BoxCollider redSpawnPoint;
    public BoxCollider blueSpawnPoint;

    [Header("Prefabs")]
    [Tooltip("Resources 폴더 안의 Player 프리팹")]
    public GameObject playerPrefab;
    [Tooltip("HUD UI Canvas 프리팹")]
    public GameObject hudCanvasPrefab;
    [Tooltip("다른 플레이어 체력바 프리팹")]
    public GameObject healthBarPrefab;

    private GameObject hudInstance;
    private Canvas mainCanvas;

    private void Awake()
    {
        if (mainCanvas == null)
            mainCanvas = FindObjectOfType<Canvas>();
    }

    private void Start()
    {
        // 모든 클라이언트에서 동일하게 실행됨
        SpawnHUD();

        // 자신 캐릭터 스폰 + 참조 반환
        var myPlayer = SpawnPlayer();

        // HUD 연결
        BindHUD(myPlayer);
    }

    //  HUD 생성
    void SpawnHUD()
    {
        if (hudInstance == null && hudCanvasPrefab != null)
            hudInstance = Instantiate(hudCanvasPrefab);
    }

    //  플레이어 생성
    GameObject SpawnPlayer()
    {
        // 이미 생성되어 있으면 중복 방지
        if (PhotonNetwork.LocalPlayer.TagObject != null)
            return PhotonNetwork.LocalPlayer.TagObject as GameObject;

        int myTeam = GetMyTeam(); // 0=RED, 1=BLUE

        // 팀별 스폰 포인트 지정
        BoxCollider area = (myTeam == 0) ? redSpawnPoint : blueSpawnPoint;
        if (area == null) area = redSpawnPoint ?? blueSpawnPoint;

        Vector3 spawnPos = (area != null) ? RandomPointInBox(area) : transform.position;
        Quaternion spawnRot = (myTeam == 0)
            ? Quaternion.LookRotation(-Vector3.forward, Vector3.up)
            : Quaternion.LookRotation(Vector3.forward, Vector3.up);

        //  네트워크 Instantiate (모든 클라 자동 복제)
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, spawnRot);
        Debug.Log($"[GameManager] Spawned {playerObj.name} for {PhotonNetwork.LocalPlayer.NickName}");

        //  서버 아닌 클라에서는 ServerMotor 강제 비활성화 (이중 보호)
        var sm = playerObj.GetComponent<ServerMotor>();
        if (sm != null && !PhotonNetwork.IsMasterClient)
            sm.enabled = false;

        //  팀 데이터 동기화 (AllBuffered)
        var team = playerObj.GetComponent<PlayerTeam>();
        if (team != null)
            team.SetTeamNetworked(myTeam); // 내부가 AllBuffered RPC여야 함

        //  내 플레이어 캐싱
        PhotonNetwork.LocalPlayer.TagObject = playerObj;

        // CaptureManager 연동
        var cap = FindObjectOfType<CapturePointManager>();
        if (cap != null && team != null)
            cap.myPlayer = team;

        return playerObj;
    }

    // 다른 플레이어 HP 바 자동 연결
    public void BindOtherHPBar(GameObject player)
    {
        if (healthBarPrefab == null || mainCanvas == null || player == null) return;

        var ph = player.GetComponent<PlayerHealth_Server>();
        if (ph == null) return;

        // 이미 바인딩된 경우 중복 생성 방지
        var exist = mainCanvas.GetComponentsInChildren<OtherPlayerHealthBar>(true);
        for (int i = 0; i < exist.Length; i++)
        {
            if (exist[i] != null && exist[i].playerHealth == ph)
                return;
        }

        var go = Instantiate(healthBarPrefab, mainCanvas.transform);
        var hpUI = go.GetComponent<OtherPlayerHealthBar>();
        if (hpUI == null) return;

        hpUI.playerHealth = ph;
        Transform head = player.transform.Find("Headup");
        hpUI.target = (head != null) ? head : player.transform;
        hpUI.cam = Camera.main;
    }

    //팀 계산
    int GetMyTeam()
    {
        var lp = PhotonNetwork.LocalPlayer;
        if (lp != null && lp.CustomProperties != null)
        {
            object v;
            if (lp.CustomProperties.TryGetValue("MyTeam", out v) ||
                lp.CustomProperties.TryGetValue("Team", out v) ||
                lp.CustomProperties.TryGetValue("team", out v))
            {
                if (v is int i) return Mathf.Clamp(i, 0, 1);
                if (v is byte b) return Mathf.Clamp((int)b, 0, 1);
            }
        }

        // 짝수=RED(0), 홀수=BLUE(1)
        int actor = (lp != null) ? lp.ActorNumber : Random.Range(1, 9999);
        return (actor % 2 == 0) ? 0 : 1;
    }

    // 랜덤 스폰 위치
    Vector3 RandomPointInBox(BoxCollider box)
    {
        Vector3 c = box.center;
        Vector3 e = box.size * 0.5f;
        Vector3 local = new Vector3(Random.Range(-e.x, e.x), 0f, Random.Range(-e.z, e.z));
        return box.transform.TransformPoint(c + local);
    }

    // HUD 바인딩
    void BindHUD(GameObject myPlayerObj)
    {
        if (hudInstance == null || myPlayerObj == null) return;

        var hp = myPlayerObj.GetComponent<PlayerHealth_Server>();
        var mgr = myPlayerObj.GetComponent<PlayerSkillManager_Net>();

        var pHud = hudInstance.GetComponentInChildren<PlayerHUD>(true);
        if (pHud != null) pHud.Init(hp, mgr);

        var cap = FindObjectOfType<CapturePointManager>();
        var capUI = hudInstance.GetComponentInChildren<CaptureUIManager>(true);
        if (capUI != null) capUI.Init(cap);
    }

    // 리스폰 처리 (서버 전용)
    public void RequestRespawn(PhotonView target, float delaySeconds)
    {
        if (target == null) return;
        if (!PhotonNetwork.IsMasterClient) return;
        StartCoroutine(RespawnRoutine(target, Mathf.Max(0f, delaySeconds)));
    }

    IEnumerator RespawnRoutine(PhotonView target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target == null) yield break;

        int team = GetTeamOf(target);

        BoxCollider area = (team == 0) ? redSpawnPoint : blueSpawnPoint;
        if (area == null) area = redSpawnPoint ?? blueSpawnPoint;

        Vector3 spawnPos = (area != null) ? RandomPointInBox(area) : target.transform.position;
        Vector3 forward = (team == 0) ? -Vector3.forward : Vector3.forward;

        var ph = target.GetComponent<PlayerHealth_Server>();
        int hp = (ph != null) ? ph.maxHealth : 100;

        target.RPC("RPC_RespawnAt", RpcTarget.AllViaServer, spawnPos, forward, hp, 1.0f);
    }

    int GetTeamOf(PhotonView target)
    {
        var pt = target.GetComponent<PlayerTeam>();
        if (pt != null) return Mathf.Clamp(pt.team, 0, 1);

        int actor = (target.Owner != null) ? target.Owner.ActorNumber : Random.Range(1, 9999);
        return (actor % 2 == 0) ? 0 : 1;
    }
}
