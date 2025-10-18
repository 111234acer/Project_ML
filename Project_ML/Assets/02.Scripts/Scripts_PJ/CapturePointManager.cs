using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections.Generic;

public class CapturePointManager : MonoBehaviour, IPunObservable
{
    public float preLockDuration = 30f;     //시작 대기 시간
    public float sustainToFlip = 10f;       // 점령하기위한 유지 시간
    [Range(0f, 100f)] public float checkpoint = 50f;    //50%유지
    public float timeToFill100 = 120f;      //점령 총시간 2분
    public float overtimeBonus = 5f;        // 추가시간

    public Transform captureCenter;       // 없으면 this.transform
    public float captureRadius = 5f;      // 없으면 콜라이더로 자동 추정

    public PlayerTeam myPlayer;
    private Collider areaCol;
    const float insideEps = 1e-4f;

    private int redInside = 0;
    private int blueInside = 0;
    private bool amInside = false;

    private int ownerTeam = -1;

    private float redScore = 0f;
    private float blueScore = 0f;

    
    private int flipCandidateTeam = -1;
    private float flipTimer = 0f;
        
    private float preLockTimer;
    private float totalGameTime = 0f;

    private float overtimeRemain = 0f;

    private bool gameEnded = false;

    private float FillPerSecond => 100f / Mathf.Max(1f, timeToFill100);

    //ui매니저로 가
    public int OwnerTeam => ownerTeam;
    public int FlipCandidateTeam => flipCandidateTeam;
    public float FlipTimer => flipTimer;
    public bool AmInside => amInside;
    public float PreLockTimer => preLockTimer;
    public float TotalGameTime => totalGameTime;
    public float RedScore => redScore;
    public float BlueScore => blueScore;
    public float OvertimeRemain => overtimeRemain;
    public bool GameEnded => gameEnded;

    PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (captureCenter == null) captureCenter = transform;

        areaCol = GetComponent<Collider>();                 // (+)
        //  반경 자동추정은 '콜라이더 없을 때만' 폴백으로 남겨둠
        if (areaCol == null && captureRadius <= 0f)
            captureRadius = 5f;

        if (pv != null)
        {
            if (pv.ObservedComponents == null) pv.ObservedComponents = new System.Collections.Generic.List<Component>();
            if (!pv.ObservedComponents.Contains(this)) pv.ObservedComponents.Add(this);
        }
    }

    void Start()
    {
        preLockTimer = preLockDuration;
    }

    void Update()
    {
        totalGameTime += Time.deltaTime;

        UpdateAmInsideForLocal();

        if (!PhotonNetwork.IsMasterClient) return;

        SampleInsideCounts();

        if (preLockTimer > 0f)
        {
            preLockTimer = Mathf.Max(0f, preLockTimer - Time.deltaTime);
            return;
        }


        HandleFlipLogic();
        HandleScoreGain();
        HandleNinetyNineBonus();
        CheckWinCondition();
    }
    bool IsInsideArea(Vector3 worldPos)
    {
        if (areaCol != null)
        {
            Vector3 cp = areaCol.ClosestPoint(worldPos);
            return (cp - worldPos).sqrMagnitude <= insideEps; // 내부면 동일점이 반환됨
        }

        // 콜라이더가 없을 때만 구형 반경(3D)으로 폴백
        Vector3 c = (captureCenter != null) ? captureCenter.position : transform.position;
        Vector3 d = worldPos - c;
        return d.sqrMagnitude <= captureRadius * captureRadius;
    }

    void UpdateAmInsideForLocal()
    {
        if (myPlayer == null) 
        { 
            myPlayer = FindObjectOfType<PlayerTeam>();

            if (myPlayer == null) 
            { 
                amInside = false; return; 
            } 
        }

        bool dead = false;
        var hpC = myPlayer.GetComponent<PlayerHealth_Server>();
        dead = (hpC != null && hpC.isDead);

        if (dead) 
        { 
            amInside = false; return;
        }

        amInside = IsInsideArea(myPlayer.transform.position);
    }

    void SampleInsideCounts()
    {
        redInside = 0;
        blueInside = 0;

        var players = FindObjectsOfType<PlayerTeam>();


        for (int i = 0; i < players.Length; i++)
        {
            var hpCopy = players[i].GetComponent<PlayerHealth_Server>();
            bool isDead = (hpCopy != null && hpCopy.isDead);
            if (isDead) continue;

            if (!IsInsideArea(players[i].transform.position)) continue;

            if (players[i].team == 0) redInside++;
            else if (players[i].team == 1) blueInside++;
        }
    }

    private void HandleFlipLogic()
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

            if (prev == 0) redScore = Mathf.Min(redScore, checkpoint);
            else if (prev == 1) blueScore = Mathf.Min(blueScore, checkpoint);
        }
    }

    private void HandleScoreGain()
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
    private void HandleNinetyNineBonus()
    {
        bool contesting = IsEnemyInside(ownerTeam);

        if (ownerTeam == 0 && redScore >= 99f && contesting)
            overtimeRemain = overtimeBonus;

        if (ownerTeam == 1 && blueScore >= 99f && contesting)
            overtimeRemain = overtimeBonus;
    }

    private bool IsEnemyInside(int team)
    {
        if (team == 0) return blueInside > 0; // 레드팀일 때 블루가 들어왔는지
        if (team == 1) return redInside > 0; // 블루팀일 때 레드가 들어왔는지
        return false;
    }

    private void CheckWinCondition()
    {
        if (gameEnded) return;

        bool contesting = IsEnemyInside(ownerTeam);

        if (overtimeRemain > 0f || contesting)
            return;

        if (redScore >= 100f)
        {
            EndGame(0);
        }
        else if (blueScore >= 100f)
        {
            EndGame(1);
        }
    }

    private void EndGame(int winningTeam)
    {
        gameEnded = true;

        if (myPlayer != null)
        {
            bool isWin = (myPlayer.team == winningTeam);
            var ui = FindObjectOfType<CaptureUIManager>();
            if (ui != null)
            {
                ui.ShowEndPanel(isWin);
            }
        }

        StartCoroutine(GoToLobbyAfterDelay(3f));
    }

    private IEnumerator GoToLobbyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("PhotonLobby");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ownerTeam);
            stream.SendNext(flipCandidateTeam);
            stream.SendNext(flipTimer);
            stream.SendNext(preLockTimer);
            stream.SendNext(totalGameTime);
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
            preLockTimer = (float)stream.ReceiveNext();
            totalGameTime = (float)stream.ReceiveNext();
            redScore = (float)stream.ReceiveNext();
            blueScore = (float)stream.ReceiveNext();
            overtimeRemain = (float)stream.ReceiveNext();
            gameEnded = (bool)stream.ReceiveNext();
        }
    }
}
