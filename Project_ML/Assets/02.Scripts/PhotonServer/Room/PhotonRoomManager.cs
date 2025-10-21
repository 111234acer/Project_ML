using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    // RPC 호출을 위한 PhotonView
    private PhotonView pv;

    [Header("Room Info GUI")]
    // 접속된 플레이어 수를 표시할 Text UI 항목 변수
    public TextMeshProUGUI playerCountText;
    // 방 나가기 버튼
    public Button ExitRoomBtn;
    public Button startGameBtn; // 게임 시작버튼 (방장한테만 보이도록)
    public TextMeshProUGUI logMsgText; // 테스트용 로그 메시지
    public Button readyBtn;

    // 팀 관련 UI
    [Header("Red Team GUI")]
    public Button selRedTeamBtn;    // 레드 팀 선택 버튼
    public Transform rtContentTr;   // Scroll View for Red Team Member Node 

    [Header("Blue Team GUI")]
    public Button selBlueTeamBtn;   // 블루 팀 선택 버튼
    public Transform btContentTr;   // Scroll View for Blue Team Member Node 

    [Header("Player Info GUI")]
    public GameObject playerNodePrefab;

    ExitGames.Client.Photon.Hashtable selTeamProps = new ExitGames.Client.Photon.Hashtable(); // (네트워크)유저 팀 선택 정보
    ExitGames.Client.Photon.Hashtable playerReady = new ExitGames.Client.Photon.Hashtable();  // (네트워크)유저 레디 상태 정보

    const string redTeam = "red";
    const string blueTeam = "blue";
    const string userTeamKey = "MyTeam";
    const string readyKey = "IamReady";

    internal string localPlayerTeam = "blue";

    const string inGameScene = "Scene_ClientTest";


    private void Awake()
    {
        readyBtn.gameObject.SetActive(false);

        pv = GetComponent<PhotonView>();

        // 모든 클라우드의 네트워크 메세지 수신을 다시 연결
        PhotonNetwork.IsMessageQueueRunning = true;
        PhotonNetwork.AutomaticallySyncScene = true; // 추가


        // 방에 입장 후 기존 접속자 수 나타내기 ( 현재인원 / 입장가능한총인원 )
        GetConnectPlayerCount();
        startGameBtn.gameObject.SetActive(false);
    }

    void Start()
    {
        // Red Team 선택 버튼
        if (selRedTeamBtn != null)
        { 
            selRedTeamBtn.onClick.AddListener(() =>
            {
                SendSelTeam(redTeam);
            });
        }

        // Blue Team 선택 버튼
        if (selBlueTeamBtn != null)
        {
            selBlueTeamBtn.onClick.AddListener(() =>
            {
                SendSelTeam(blueTeam);
            });
        }

        if(readyBtn != null)
        {
            readyBtn.onClick.AddListener(() =>
            {
                SendReady();
            });
        }

        startGameBtn.onClick.AddListener(() =>
        {
            GameStart();
            //pv.RPC("GameStart", RpcTarget.AllBuffered);
        });

        ExitRoomBtn.onClick.AddListener(() =>
        {
            OnClickExitRoom();
        });

        // 방 만드는 입장에서는 로비에서 이미 OnJoinedRoom이 호출된다. 따라서 방장은 여기서 프로퍼티를 초기화한다.
        // 만들어진 방에 입장하는 사람은 이 씬에서 OnJoinedRoom이 호출된다.
        if(PhotonNetwork.IsMasterClient)
        {
            InitSelTeamProps();
            InitReadyProps();
        }
    }

    void Update()
    {
        if (IsGamePossible() == false)
            return;

        if(IsUpdatedPlayer())   // 방 참여자들의 정보나 상태가 변경되었다면...
            RefreshTeamState(); // 유저 정보 업데이트

        if (PhotonNetwork.IsMasterClient)
            CheckPossibleStart();
    }

    public override void OnJoinedRoom()
    {
        InitSelTeamProps();
        InitReadyProps();
    }

    // 방 접속자 수를 조회하는 함수
    void GetConnectPlayerCount()
    {
        // 현재 입장한 방 정보를 받아옴
        Room currentRoom = PhotonNetwork.CurrentRoom; // using Photon.Realtime;

        // 현재 방의 접속자 수와 최대 접속 가능한 수를 문자열로 구성한 후 Text UI 항목에 출력
        playerCountText.text = currentRoom.PlayerCount.ToString() + "/" + currentRoom.MaxPlayers.ToString();
    }

    #region ---------------- 팀선택 동기화 처리
    void InitSelTeamProps()
    {
        // 속도를 위해 버퍼를 미리 만들어 놓는다는 의미
        selTeamProps.Clear();
        selTeamProps.Add(userTeamKey, blueTeam);   // 기본적으로 나는 블루팀으로 시작한다.
        PhotonNetwork.LocalPlayer.SetCustomProperties(selTeamProps);
        Debug.Log("프로퍼티 설정 완료");
        // 캐릭터 별로 동기화 시키고 싶은 경우
    }

    // 팀 선택
    void SendSelTeam(string a_Team)
    {
        if (string.IsNullOrEmpty(a_Team) == true)
            return;

        if (selTeamProps == null)
        {
            selTeamProps = new ExitGames.Client.Photon.Hashtable();
            selTeamProps.Clear();
        }

        if (selTeamProps.ContainsKey(userTeamKey) == true)
        {
            selTeamProps[userTeamKey] = a_Team;
        }
        else
        {
            selTeamProps.Add(userTeamKey, a_Team);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(selTeamProps);
    }

    // 유저들 팀소속 정보 가져오기
    string ReceiveSelTeam(Photon.Realtime.Player a_Player) 
    {
        string playerTeam = blueTeam;
        if (a_Player == null)
            return playerTeam;

        if (a_Player.CustomProperties.ContainsKey(userTeamKey) == true)
            playerTeam = (string)a_Player.CustomProperties[userTeamKey];

        return playerTeam;
    }
    #endregion

    #region Ready 상태 동기화 함수
    void InitReadyProps()
    {
        // 속도를 위해 버퍼를 미리 만들어 놓는다는 의미
        playerReady.Clear();
        playerReady.Add(readyKey, 0); // 기본적으로 아직 준비전 상태로 시작
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerReady);

        // 프로퍼티 정보가 초기화 되기전에 레디버튼을 누르면 에러뜸.
        readyBtn.gameObject.SetActive(true);
    }

    // 레디 신호 보내기
    void SendReady()
    {

        if (playerReady == null)
        {
            playerReady = new ExitGames.Client.Photon.Hashtable();
            playerReady.Clear();
        }

        if (playerReady.ContainsKey(readyKey) == true)
        {
            // 레디 상태가 아니었다면 -> 레디 상태로 전환
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties[readyKey] == 0)
            {
                playerReady[readyKey] = 1;
                ExitRoomBtn.gameObject.SetActive(false);
            }
            else // 레디 상태였다면 -> 레디 풀기
            {
                playerReady[readyKey] = 0;
                ExitRoomBtn.gameObject.SetActive(true);
            }
        }
        else
        {
            playerReady.Add(readyKey, 1);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerReady);
    }

    // 레디 신호 받기
    bool ReceiveReady(Photon.Realtime.Player a_Player) // Ready 상태를 받아서 처리하는 부분
    {
        if (a_Player == null)
            return false;
        if (a_Player.CustomProperties.ContainsKey(readyKey) == false)
            return false;

        if ((int)a_Player.CustomProperties[readyKey] == 0)
            return false;
        else
        {
            return true;
        }
    }
    #endregion

    void RefreshTeamState()
    {
        // 모든 멤버 노드 삭제
        for(int i =0; i < rtContentTr.childCount; i++)
        {
            Destroy(rtContentTr.GetChild(i).gameObject);
        }

        for (int i = 0; i < btContentTr.childCount; i++)
        {
            Destroy(btContentTr.GetChild(i).gameObject);
        }

        string playerTeam = blueTeam;

        foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            playerTeam = ReceiveSelTeam(player);
            GameObject playerNode = Instantiate(playerNodePrefab);

            if(playerTeam == blueTeam)
            {
                playerNode.transform.SetParent(btContentTr);
            }
            else if(playerTeam == redTeam)
            {
                playerNode.transform.SetParent(rtContentTr);
            }


            PhotonPlayerNode photonPlayerNode = playerNode.GetComponent<PhotonPlayerNode>();
            photonPlayerNode.uniqueId = player.ActorNumber; // 포톤에서 발급하는 유저고유넘버
            photonPlayerNode.team = playerTeam;
            photonPlayerNode.isReady = ReceiveReady(player);
            photonPlayerNode.nickname = player.NickName;
            // PhotonNetwork.LocalPlayer.ActorNumber 는 포톤이 나에게 발급한 고유넘버
            bool isLocal = (photonPlayerNode.uniqueId == PhotonNetwork.LocalPlayer.ActorNumber); // 현재 체크하는 Player가 로컬인지 검사

            if (player.IsMasterClient) // 방장이라면...
            {
                photonPlayerNode.isHost = true; // 호스트 표시 켜기
            }

            if (isLocal)
            {
                localPlayerTeam = photonPlayerNode.team; // 로컬 플레이어가 어느팀인지 기억해두기.
            }
        }

        // (로컬)레디 상태에서는 팀 이동 불가
        if(ReceiveReady(PhotonNetwork.LocalPlayer) == true)
        {
            selRedTeamBtn.gameObject.SetActive(false);
            selBlueTeamBtn.gameObject.SetActive(false);
        }
        else // 레디를 안한 상태라면 상대팀으로 이동하는 버튼만 켜기
        {
            playerTeam = ReceiveSelTeam(PhotonNetwork.LocalPlayer);
            if(playerTeam == blueTeam)
            {
                selRedTeamBtn.gameObject.SetActive(true);
                selBlueTeamBtn.gameObject.SetActive(false);
            }
            else
            {
                selRedTeamBtn.gameObject.SetActive(false);
                selBlueTeamBtn.gameObject.SetActive(true);
            }
        }
    }

    bool IsUpdatedPlayer() // 방에 참여한 플레이어의 정보가 달라졌는지 확인 (true라면 방 멤버 정보를 업데이트 해야함.)
    {
        PhotonPlayerNode[] ppnArray = FindObjectsOfType<PhotonPlayerNode>();

        if (ppnArray == null)
            return true;

        // 디스플레이 되고 있는 플레이어 수와 실제 플레이어 수가 다르다면
        if (PhotonNetwork.PlayerList.Length != ppnArray.Length)
            return true;

        foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            bool isExistNode = false;
            foreach(PhotonPlayerNode node in ppnArray)
            {
                if (node == null)
                    continue;

                if(node.uniqueId == player.ActorNumber) // ActorNumber는 방 안에 있는 유저들의 Identity를 나타낸다. -1이라면 방 밖에 있다는 뜻.
                {
                    // 해당 유저의 팀 선택 정보가 바뀌었다면...
                    if (node.team != ReceiveSelTeam(player))
                        return true;

                    // 해당 유저의 레디 상태가 변경되었다면...
                    if (node.isReady != ReceiveReady(player))
                        return true;

                    isExistNode = true;
                    break;
                }
            }

            // 보통 유저가 방에서 나간 경우
            if (isExistNode == false)
                return true;
        }
        return false;
    }   
    bool IsGamePossible() // 게임이 가능한 상태인지? 체크하는 함수
    {
        //나가는 타이밍에 포톤 정보들이 한프레임 먼저 사라지고 LoadScene()이 한프레임 늦게 호출되는 문제 해결법
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.LocalPlayer == null)
            return false; // 동기화 가능한 상태일 때만 업데이트를 계산해 준다.

        return true;
    }

    // 참가 유저 모두 Ready 버튼 눌렀는지 검사
    // 각 팀의 유저 수가 맞는지 검사
    // 모든 조건이 충족됐다면 방장의 Start버튼 활성화
    void CheckPossibleStart()
    {
        startGameBtn.gameObject.SetActive(false);

        bool isAllReady = true;
        int redTeamCount = 0;
        int blueTeamCount = 0;
        foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            // 한 사람이라도 레디 상태가 아니라면...
            if (ReceiveReady(player) == false)
            { 
                isAllReady = false;
                break;
            }

            if(ReceiveSelTeam(player) == redTeam)
            {
                redTeamCount++;
            }
            else
            {
                blueTeamCount++;
            }
        }

        if (isAllReady == true) // 모두 인원이 레디 상태라면...
        {
            // 각 팀의 인원수가 동일한지 검사...
            if(redTeamCount == blueTeamCount || redTeamCount != blueTeamCount)
            {
                startGameBtn.gameObject.SetActive(true);
            }
        }
    }

    void GameStart()
    {
        // (이 부분은 방의 모든 멤버가 호출) 누가 발생시켰든 동기화
        if (PhotonNetwork.CurrentRoom.IsOpen == true)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false; // 게임이 시작되면 다른 유저들이 들어오지 못하도록 막는 부분
            PhotonNetwork.CurrentRoom.IsVisible = false; // 로비에서 방 목록에서도 보이지 않게 하기
        }
        PhotonNetwork.IsMessageQueueRunning = false;
        // 인게임으로 이동...
        PhotonNetwork.LoadLevel(inGameScene); //  수정
    }

    // 방 나가기
    public void OnClickExitRoom()
    {
        // 방장 나갈 때 호스트 넘겨주고 나가기
        if(PhotonNetwork.IsMasterClient)
        {
            foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if(player.IsMasterClient == false)
                {
                    PhotonNetwork.SetMasterClient(player);
                }
            }
        }

        // 로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#ff0000>[" + PhotonNetwork.LocalPlayer.NickName + "] Disconnected</color>";
        // RPC 함수 호출
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg);
        // 설정이 완료된 후 빌드 파일을 여러개 실행해
        // 동일한 방에 입장해보면 접속 로그가 표기되는 것을 확인할 수 있다.
        // 또한 PhotonTarget.AllBuffered 옵션으로
        // RPC를 호출했기 때문에 나중에 입장해도 기존의 접속 로그 메시지가 뜬다.
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void LogMsg(string msg)
    {
        // 로그 메세지 Text UI에 텍스트를 누적시켜 표시
        logMsgText.text = logMsgText.text + msg;
    }

    // 방에서 접속 종료됐을 때 호출되는 콜백 함수
    public override void OnLeftRoom() // PhotonNetwork.LeaveRoom(); 성공했을 때
    {
        // 로비 씬을 호출
        UnityEngine.SceneManagement.SceneManager.LoadScene("PhotonLobby");
    }


}
