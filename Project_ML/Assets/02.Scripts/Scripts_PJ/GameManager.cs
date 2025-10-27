using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    public BoxCollider redSpawnPoint;
    public BoxCollider blueSpawnPoint;

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

        // [SPAWN SAFETY] 스폰 후 내 플레이어 참조를 반환받아 HUD 바인딩까지 한 번에
        var myPlayer = SpawnPlayer();

        BindHUD(myPlayer);
    }

    void SpawnHUD()
    {
        if (hudInstance == null && hudCanvasPrefab != null)
            hudInstance = Instantiate(hudCanvasPrefab);
    }

    GameObject SpawnPlayer()
    {
        if (PhotonNetwork.LocalPlayer.TagObject != null)
            return PhotonNetwork.LocalPlayer.TagObject as GameObject;

        int myTeam = GetMyTeam(); // 0=RED, 1=BLUE

        // 팀별 스폰 영역 선택
        BoxCollider area = (myTeam == 0) ? redSpawnPoint : blueSpawnPoint;
        if (area == null) area = redSpawnPoint ?? blueSpawnPoint;

        Vector3 spawnPos = (area != null) ? RandomPointInBox(area) : transform.position;
        Quaternion spawnRot = (myTeam == 0)
            ? Quaternion.LookRotation(-Vector3.forward, Vector3.up)
            : Quaternion.LookRotation(Vector3.forward, Vector3.up);

        int charId = -1;
        var lp = PhotonNetwork.LocalPlayer;
        if (lp != null && lp.CustomProperties != null &&
            lp.CustomProperties.TryGetValue("Char", out var v) && v is int ci)
            charId = ci;

        var data = CharacterCatalog.Instance.Get(charId);

        if (string.IsNullOrEmpty(data.prefabName))
        {
            Debug.LogError("[GameManager] No prefab to spawn. Check CharacterCatalog.prefabName or playerPrefab.");
            return null;
        }

        // 네트워크 Instantiate 모든 클라 자동 복제     
        var playerObj = PhotonNetwork.Instantiate(data.prefabName, spawnPos, spawnRot);

        // 서버아닌 클라에서는 ServerMotor 강제 비활성화
        var sm = playerObj.GetComponent<ServerMotor>();
        if (sm != null && !PhotonNetwork.IsMasterClient)
            sm.enabled = false;


        // 팀 데이터 동기화
        var playerTeam = playerObj.GetComponent<PlayerTeam>();
        if (playerTeam != null)
            playerTeam.SetTeamNetworked(myTeam);

        // 내 플레이어 캐시
        PhotonNetwork.LocalPlayer.TagObject = playerObj;

        var cap = FindObjectOfType<CapturePointManager>();
        if (cap != null) cap.myPlayer = playerTeam;

        // 다른 플레이어 HP바 바인딩은 나중에 Join/Instantiate 이벤트에서 개별 처리 가능
        return playerObj;
    }

    public void BindOtherHPBar(GameObject player)
    {
        if (HealthBar == null || mainCanvas == null || player == null) return;

        var ph = player.GetComponent<PlayerHealth_Server>();
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

        hpUI.playerHealth = ph;
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

        int actor = (lp != null) ? lp.ActorNumber : Random.Range(1, 9999);
        return (actor % 2 == 0) ? 0 : 1; // 짝수=RED(0), 홀수=BLUE(1)
    }

    Vector3 RandomPointInBox(BoxCollider box)
    {
        Vector3 c = box.center;
        Vector3 e = box.size * 0.5f;
        Vector3 local = new Vector3(Random.Range(-e.x, e.x), 0f, Random.Range(-e.z, e.z));
        return box.transform.TransformPoint(c + local);
    }

    // [BIND FIX] 내 플레이어 기준으로 HUD 연결 (기존 FindObjectOfType 의 모호성 제거)
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
        var ph = target.GetComponent<PlayerHealth_Server>();
        int hp = (ph != null) ? ph.maxHealth : 100;

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
