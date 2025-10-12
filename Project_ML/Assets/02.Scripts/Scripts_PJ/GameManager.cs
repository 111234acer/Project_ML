using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviour
{
    public BoxCollider redSpawnPoint;
    public BoxCollider blueSpawnPoint;

    public GameObject playerPrefab;
    public GameObject hudCanvasPrefab;

    private GameObject hudInstance;

    [SerializeField] Canvas mainCanvas;
    [SerializeField] GameObject HealthBar;

    private void Awake()
    {
        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
    }

    private void Start()
    {
        SpawnHUD();

        SpawnPlayer();

        BindHUD();
    }
    void SpawnHUD()
    {
        if (hudInstance == null && hudCanvasPrefab != null)
            hudInstance = Instantiate(hudCanvasPrefab);
    }

    void SpawnPlayer()
    {
        if (FindObjectOfType<PlayerTeam>() != null) return;

        // 1) 내 팀 구하기(커스텀 프로퍼티 우선, 없으면 안전한 폴백)
        int myTeam = GetMyTeam(); // 0=RED, 1=BLUE

        // 2) 팀별 스폰 영역 선택
        BoxCollider area = (myTeam == 0) ? redSpawnPoint : blueSpawnPoint;
        if (area == null) area = redSpawnPoint ?? blueSpawnPoint;

        // 폴백
        if (area == null) area = redSpawnPoint ?? blueSpawnPoint;

        // 3) 영역 안 좌표를 먼저 뽑아서 그 위치로 네트워크 생성
        Vector3 spawnPos = (area != null) ? RandomPointInBox(area) : transform.position;
        Quaternion spawnRot = (myTeam == 0)
            ? Quaternion.LookRotation(-Vector3.forward, Vector3.up)   // RED 기본 방향
            : Quaternion.LookRotation(Vector3.forward, Vector3.up);  // BLUE 기본 방향

        var playerObj = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, spawnRot);
        BindOtherHPBar(playerObj);

        // 4) 플레이어 컴포넌트에 팀 주입(AssignTeam()이 로컬 계산이면 건너뛰고 직접 세팅)
        var player = playerObj.GetComponent<PlayerTeam>();
        if (player != null)
        {
            player.SetTeamNetworked(myTeam); //핵심: RpcTarget.AllBuffered로 모든 클라에 동일 반영
        }

        var cap = FindObjectOfType<CapturePointManager>();
        if (cap != null) cap.myPlayer = player;
    }
    public void BindOtherHPBar(GameObject player)
    {
        if (HealthBar == null || mainCanvas == null || player == null) return;

        var ph = player.GetComponent<PlayerHealth_Copy>();
        if (ph == null) return;

        // 이미 바인딩된 UI가 있으면 생성하지 않음 (중복 방지)
        var exist = mainCanvas.GetComponentsInChildren<OtherPlayerHealthBar>(true);
        for (int i = 0; i < exist.Length; i++)
        {
            if (exist[i] != null && exist[i].playerHealth == ph)
                return;
        }

        var go = Instantiate(HealthBar, mainCanvas.transform);
        var hpUI = go.GetComponent<OtherPlayerHealthBar>();
        if (hpUI == null) return;

        hpUI.playerHealth = ph; // 타입: PlayerHealth_Copy
        Transform head = player.transform.Find("Headup");
        hpUI.target = (head != null) ? head : player.transform;
        hpUI.cam = Camera.main; // (없어도 스크립트가 지연 할당)
    }


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

        // 폴백: 배우번호 기반 균등 분배(중복 최소화)
        int actor = (lp != null) ? lp.ActorNumber : Random.Range(1, 9999);
        return (actor % 2 == 0) ? 0 : 1; // 짝수=RED(0), 홀수=BLUE(1)
    }

    Vector3 RandomPointInBox(BoxCollider box)
    {
        Vector3 c = box.center;
        Vector3 e = box.size * 0.5f;
        Vector3 local = new Vector3(Random.Range(-e.x, e.x),0f, Random.Range(-e.z, e.z));
        return box.transform.TransformPoint(c + local);
    }

    void BindHUD()
    {
        if (hudInstance == null) return;

        var player = FindObjectOfType<PlayerTeam>();
        if (player == null) return;

        var hp = player.GetComponent<PlayerHealth_Copy>();
        var mgr = player.GetComponent<PlayerSkillManager_Copy>();

        // PlayerHUD 연결
        var pHud = hudInstance.GetComponentInChildren<PlayerHUD>(true);
        if (pHud != null) pHud.Init(hp, mgr);

        // Capture UI 연결
        var cap = FindObjectOfType<CapturePointManager>();
        var capUI = hudInstance.GetComponentInChildren<CaptureUIManager>(true);
        if (capUI != null) capUI.Init(cap);
    }

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

        // 팀별 스폰 영역
        BoxCollider area = (team == 0) ? redSpawnPoint : blueSpawnPoint;
        if (area == null) area = redSpawnPoint ?? blueSpawnPoint;

        // 좌표/회전
        Vector3 spawnPos = (area != null) ? RandomPointInBox(area) : target.transform.position;
        Vector3 forward = (team == 0) ? -Vector3.forward : Vector3.forward;

        // 체력 상한
        var ph = target.GetComponent<PlayerHealth_Copy>();
        int hp = (ph != null) ? ph.MaxHealth : 100;

        // 전체 동기화
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