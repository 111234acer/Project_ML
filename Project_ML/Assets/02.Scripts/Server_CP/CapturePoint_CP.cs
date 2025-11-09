using UnityEngine;
using Photon.Pun;

/// <summary>
/// 클라이언트 권위형 점령 시스템
/// CapturePointManager와 동일한 기능을 클라이언트에서 처리
/// </summary>
public class CapturePoint_CP : MonoBehaviourPun, IPunObservable
{
    [Header("Capture Settings")]
    public float preLockDuration = 30f;   // 시작 전 대기
    public float sustainToFlip = 10f;     // 점령 유지 시간
    [Range(0f, 100f)] public float checkpoint = 50f; // 점령 반 유지선
    public float timeToFill100 = 120f;    // 100%까지 걸리는 시간
    public float overtimeBonus = 5f;      // 오버타임 연장 시간

    [Header("Area Settings")]
    public Transform captureCenter;
    public float captureRadius = 5f;

    [Header("Runtime Values (읽기용)")]
    public int ownerTeam = -1;
    public int flipCandidateTeam = -1;
    public float flipTimer = 0f;
    public float redScore = 0f;
    public float blueScore = 0f;
    public float preLockTimer;
    public float totalGameTime;
    public float overtimeRemain;
    public bool gameEnded = false;

    float FillPerSecond => 100f / Mathf.Max(1f, timeToFill100);
    const float insideEps = 1e-4f;

    int redInside = 0;
    int blueInside = 0;

    Collider areaCol;

    void Awake()
    {
        if (captureCenter == null) captureCenter = transform;
        areaCol = GetComponent<Collider>();

        if (pv == null) pv = GetComponent<PhotonView>();
        preLockTimer = preLockDuration;

        // Photon 동기화 등록
        if (pv != null)
        {
            if (pv.ObservedComponents == null)
                pv.ObservedComponents = new System.Collections.Generic.List<Component>();
            if (!pv.ObservedComponents.Contains(this))
                pv.ObservedComponents.Add(this);
        }
    }

    PhotonView pv;

    void Update()
    {
        totalGameTime += Time.deltaTime;
        if (gameEnded) return;

        if (preLockTimer > 0f)
        {
            preLockTimer -= Time.deltaTime;
            return;
        }

        CountPlayersInside();
        HandleFlipLogic();
        HandleScoreGain();
        HandleNinetyNineBonus();
        CheckWinCondition();
    }

    bool IsInside(Vector3 pos)
    {
        if (areaCol)
        {
            Vector3 cp = areaCol.ClosestPoint(pos);
            return (cp - pos).sqrMagnitude <= insideEps;
        }

        Vector3 c = captureCenter.position;
        Vector3 d = pos - c;
        return d.sqrMagnitude <= captureRadius * captureRadius;
    }

    void CountPlayersInside()
    {
        redInside = 0;
        blueInside = 0;

        var players = FindObjectsOfType<PlayerTeam_CP>();
        foreach (var p in players)
        {
            var health = p.GetComponent<PlayerHealth_CP>();
            if (health != null && health.currentHP <= 0f) continue;

            if (!IsInside(p.transform.position)) continue;

            if (p.teamNumber == 0) redInside++;
            else if (p.teamNumber == 1) blueInside++;
        }
    }

    void HandleFlipLogic()
    {
        bool redOnly = redInside > 0 && blueInside == 0;
        bool blueOnly = blueInside > 0 && redInside == 0;

        if (!redOnly && !blueOnly)
        {
            flipCandidateTeam = -1;
            flipTimer = 0f;
            return;
        }

        int candidate = redOnly ? 0 : 1;

        if (ownerTeam == candidate)
        {
            flipCandidateTeam = -1;
            flipTimer = 0f;
            return;
        }

        if (flipCandidateTeam != candidate)
        {
            flipCandidateTeam = candidate;
            flipTimer = 0f;
        }

        flipTimer += Time.deltaTime;

        if (flipTimer >= sustainToFlip)
        {
            int prev = ownerTeam;
            ownerTeam = candidate;
            flipCandidateTeam = -1;
            flipTimer = 0f;
            overtimeRemain = 0f;

            // 이전 팀 점수 리셋 (체크포인트까지만 유지)
            if (prev == 0) redScore = Mathf.Min(redScore, checkpoint);
            else if (prev == 1) blueScore = Mathf.Min(blueScore, checkpoint);
        }
    }

    void HandleScoreGain()
    {
        bool contesting = IsEnemyInside(ownerTeam);

        if (ownerTeam == 0)
        {
            if (redScore < 99f || (!contesting && overtimeRemain <= 0f))
                redScore += FillPerSecond * Time.deltaTime;

            if ((overtimeRemain > 0f || contesting) && redScore > 99f)
                redScore = 99f;
        }
        else if (ownerTeam == 1)
        {
            if (blueScore < 99f || (!contesting && overtimeRemain <= 0f))
                blueScore += FillPerSecond * Time.deltaTime;

            if ((overtimeRemain > 0f || contesting) && blueScore > 99f)
                blueScore = 99f;
        }

        redScore = Mathf.Clamp(redScore, 0f, 100f);
        blueScore = Mathf.Clamp(blueScore, 0f, 100f);

        if (overtimeRemain > 0f && !contesting)
            overtimeRemain = Mathf.Max(0f, overtimeRemain - Time.deltaTime);
    }

    void HandleNinetyNineBonus()
    {
        bool contesting = IsEnemyInside(ownerTeam);

        if (ownerTeam == 0 && redScore >= 99f && contesting)
            overtimeRemain = overtimeBonus;

        if (ownerTeam == 1 && blueScore >= 99f && contesting)
            overtimeRemain = overtimeBonus;
    }

    bool IsEnemyInside(int team)
    {
        if (team == 0) return blueInside > 0;
        if (team == 1) return redInside > 0;
        return false;
    }

    void CheckWinCondition()
    {
        if (overtimeRemain > 0f || IsEnemyInside(ownerTeam)) return;

        if (redScore >= 100f) EndGame(0);
        else if (blueScore >= 100f) EndGame(1);
    }

    void EndGame(int winningTeam)
    {
        gameEnded = true;
        Debug.Log($"Team {winningTeam} Win!");

        var ui = FindObjectOfType<CaptureUIManager>();
        if (ui)
        {
            // UI 갱신 (기존 로직 그대로)
            var myTeam = FindObjectOfType<PlayerTeam_CP>();
            bool isWin = (myTeam && myTeam.teamNumber == winningTeam);
            ui.ShowEndPanel(isWin);
        }

        GameManager_CP.Instance?.OnCaptureProgress_CP(redScore, blueScore);
    }

    // 네트워크 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ownerTeam);
            stream.SendNext(flipCandidateTeam);
            stream.SendNext(flipTimer);
            stream.SendNext(redScore);
            stream.SendNext(blueScore);
            stream.SendNext(overtimeRemain);
            stream.SendNext(gameEnded);
        }
        else
        {
            ownerTeam = (int)stream.ReceiveNext();
            flipCandidateTeam = (int)stream.ReceiveNext();
            flipTimer = (float)stream.ReceiveNext();
            redScore = (float)stream.ReceiveNext();
            blueScore = (float)stream.ReceiveNext();
            overtimeRemain = (float)stream.ReceiveNext();
            gameEnded = (bool)stream.ReceiveNext();
        }
    }
}
