using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoxCollider redSpawnPoint;
    public BoxCollider blueSpawnPoint;

    public GameObject playerPrefab;
    public GameObject hudCanvasPrefab;

    private GameObject hudInstance;

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
        if (FindObjectOfType<Player>() != null) return;

        var playerObj = Instantiate(playerPrefab, transform.position, transform.rotation);

        var player = playerObj.GetComponent<Player>();
        if (player != null) player.AssignTeam();

        BoxCollider area = null;
        if (player != null && player.team == 0) area = redSpawnPoint;
        else if (player != null && player.team == 1) area = blueSpawnPoint;

        if (area != null)
        {
            Vector3 pos = RandomPointInBox(area);
            playerObj.transform.position = pos; 
        }

        if (player != null)
        {
            Vector3 forward = (player.team == 0) ? -Vector3.forward : Vector3.forward;
            playerObj.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        var cap = FindObjectOfType<CapturePointManager>();
        if (cap != null) cap.myPlayer = player;
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

        var player = FindObjectOfType<Player>();
        if (player == null) return;

        var hp = player.GetComponent<PlayerHealth>();
        var mgr = player.GetComponent<PlayerSkillManager>();

        // PlayerHUD 연결
        var pHud = hudInstance.GetComponentInChildren<PlayerHUD>(true);
        if (pHud != null) pHud.Init(hp, mgr);

        // Capture UI 연결
        var cap = FindObjectOfType<CapturePointManager>();
        var capUI = hudInstance.GetComponentInChildren<CaptureUIManager>(true);
        if (capUI != null) capUI.Init(cap);
    }
}