using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class PhotonLobbyManager : MonoBehaviourPunCallbacks
{
    public static PhotonLobbyManager instance;  // 싱글톤 인스턴스
    
    string nextSceneName = "PhotonRoom";

    public TextMeshProUGUI loadingText;         // 로딩 텍스트

    public TMP_InputField nicknameIF;           // 닉네임 인풋필드
    public TMP_InputField roomNameIF;           // 방이름 인풋필드
    public Button createRoomBtn;                // 방생성 버튼
    public Button joinRandomRoomBtn;            // 랜덤 방 입장 버튼
    public Transform roomContentTr;             // 방 목록 ScrollView Transform
    public GameObject roomNodePrefab;           // 룸 목록 노드 Prefab

    public Toggle[] matchCountToggle;           // 방 인원 설정 토글

    List<RoomInfo> myRoomList = new List<RoomInfo>();

    public static PhotonLobbyManager Instance // 싱글톤 프로퍼티
    {
        get
        {
            if (instance == null)
                return null;

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true; // 추가

        //포톤 클라우드 서버 접속 여부 확인(인게임에서 빠져나온 경우가 있기 때문에...)
        if (!PhotonNetwork.IsConnected)
        {
            //1번, 포톤 클라우드에 접속
            PhotonNetwork.ConnectUsingSettings();
            //포톤 서버에 접속시도(지역 서버 접속) -> 사용자 인증 -> 로비 입장 진행
        }

        roomNameIF.text = "Room_" + Random.Range(0, 999).ToString("000");

        createRoomBtn.gameObject.SetActive(false);
        joinRandomRoomBtn.gameObject.SetActive(false);
    }

    void Start()
    {
        createRoomBtn.onClick.AddListener(() =>
        {
            CreateRoom();
        });

        joinRandomRoomBtn.onClick.AddListener(() =>
        {
            ClickJoinRandomRoom(GetUserCount());
        });
    }

    void Update()
    {
        loadingText.text = PhotonNetwork.NetworkClientState.ToString();
    }


    #region 포톤 로비 접속
    //-------------------------------------------------------------------------------------- 로비접속

    //2번, ConnectUsingSettings() 함수 호출에 대한 서버 접속이 성공하면 호출되는 콜백 함수
    //PhotonNetwork.LeaveRoom(); 으로 방을 떠날 때도 이 함수가 자동으로 호출된다.
    public override void OnConnectedToMaster()
    {   //여기서 Master는 포톤의 지역 서버를 의미한다.
        Debug.Log("서버 접속 완료");
        //단순 포톤 서버 접속만 된 상태 (ConnectedToMaster)

        //3번
        PhotonNetwork.JoinLobby();
    }

    //4번, PhotonNetwork.JoinLobby() 성공시 호출되는 로비 접속 콜백함수
    public override void OnJoinedLobby()
    {
        Debug.Log("로비접속완료");

        //무작위로 추출된 방으로 입장
        //ExitGames.Client.Photon.Hashtable roomProperties =
        //new ExitGames.Client.Photon.Hashtable() { { "map", 1 }, { "minLevel", 10 } };
        //PhotonNetwork.JoinRandomRoom(roomProperties, 4);
        //PhotonNetwork.JoinRandomRoom();

        createRoomBtn.gameObject.SetActive(true);
        joinRandomRoomBtn.gameObject.SetActive(true);
    }
    #endregion

    #region 포톤 방 접속
    //--------------------------------------------------------------------------------------- 방 접속
    public void ClickJoinRandomRoom(int playerCount)         //3번 방 입장 요청 버튼 누름
    {
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = nicknameIF.text;

        //5번 무작위로 추출된 방으로 입장
        PhotonNetwork.JoinRandomRoom(null, playerCount);
        //PhotonNetwork.JoinRandomRoom();
    }

    //PhotonNetwork.JoinRandomRoom() 이 함수 실패한 경우 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 방 참가 실패 (참가할 방이 존재하지 않습니다.)");
        CreateRoom(); // 방만들기
        // 방이 없을 때는 내가 방을 만들고 입장해 버린다.
        // ( 5번 랜덤 로그인 시에 서버 역할을 하게 될 Client는 이쪽으로 들어오게 될 것이다.)
    }

    //PhotonNetwork.CreateRoom() 이 함수가 성공하면 2번째로 자동으로 호출되는 함수
    //PhotonNetwork.JoinRoom() 함수가 성공해도 자동으로 호출되는 함수
    //PhotonNetwork.JoinRandomRoom(); 함수가 성공해도 자동으로 호출되는 함수
    public override void OnJoinedRoom()
    {
        // 서버역할인 경우         [6번 : 방입장]
        // 클라이언트 역할인 경우  [5번 : 방입장]
        Debug.Log("방 참가 완료");
        //룸 씬으로 이동하는 코루틴 실행
        //StartCoroutine(this.LoadGameScene());     //기존
        PhotonNetwork.LoadLevel(nextSceneName); //  수정
    }

    //(같은 이름의 방이 있을 때 실패함)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 만들기 실패");
        //주로 같은 이름의 방이 존재할 때 룸생성 에러가 발생된다.
        Debug.Log(returnCode.ToString()); //오류 코드(ErrorCode 클래스)
        Debug.Log(message); //오류 메시지
        CreateRoom(); // 방만들기
    }

    void CreateRoom()
    {
        //룸 생성
        PhotonNetwork.LocalPlayer.NickName = nicknameIF.text;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        int maxPlayer = GetUserCount();
        if (maxPlayer < 1)
            return;
        roomOptions.MaxPlayers = maxPlayer;
        //roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "CustomProperties", "커스텀 프로퍼티" } };
        //roomOptions.CustomRoomPropertiesForLobby = new string[] { "CustomProperties" };
        
        string _roomName = roomNameIF.text;
        // 방 이름이 없거나 Null 일 경우 방 이름 지정
        if (string.IsNullOrEmpty(roomNameIF.text))
        {
            _roomName = "ROOM_" + Random.Range(0, 999).ToString("000");
        }

        PhotonNetwork.CreateRoom(_roomName, roomOptions, null);
    }

    // RoomNode가 클릭됐을 때
    public void OnClickRoomNode(string roomName)
    {
        string _userName = nicknameIF.text;
        if (string.IsNullOrEmpty(_userName))
        {
            _userName = "USER_" + Random.Range(0, 999).ToString("000");
        }

        // 로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = _userName;

        //인자로 전달된 이름에 해당하는 룸으로 입장
        PhotonNetwork.JoinRoom(roomName);
    }
    #endregion

    IEnumerator LoadGameScene() // 최종 게임 씬 로딩 --> 6번 or 5번
    {
        //씬을 이동하는 동안 포톤 클라우드 서버로부터 네트워크 메시지 수신 중단
        PhotonNetwork.IsMessageQueueRunning = false;
        //백그라운드로 씬 로딩

        Time.timeScale = 1.0f;  //게임에 들어갈 때는 원래 속도로...

        // 기존
        AsyncOperation ao =
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(nextSceneName);

        yield return ao;

        // 수정
        PhotonNetwork.LoadLevel(nextSceneName);
        yield break;
    }

    int GetUserCount() // 매치인원설정 Toggle의 선택에 따라 방 만들때 방의 최대 인원 수 정하기.
    {
        for(int i =0; i < matchCountToggle.Length; i++)
        {
            if(matchCountToggle[i].isOn)
            {
                return (i + 1) * 2;
            }
        }

        return -1;
    }

    // 생성된 방 목록이 변경됐을 때 호출되는 콜백 함수
    // 방 리스트 갱신은 로비에서만 가능
    // 내가 로비로 진입할 때도 OnRoomListUpdate() 함수를 받고
    // 누군가 방을 새로 만들거나 방이 파괴될 때도 OnRoomListUpdate() 함수를 받음
    // A가 로비에서 대기하고 있는데 B가 방을 만들고 들어가면 OnRoomListUpdate() 가 로비에서 대기하고 있었던 A쪽에서 호출됨.
    // B가 방을 만들면서 들어갈 때는 roomList[i].RemoveFromList == false 가 되고,
    // B가 방을 떠나면서 방이 제거되야 할 때 roomList[i].RemoveFromList == true가 됨
    // A가 로그아웃(포톤서버에 접속끊기) 했다가 다시 로비까지 들어 올 때도 OnRoomListUpdate() 함수를 받게 됨.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 서버에 존재하는 방들의 정보를 살펴봅니다.
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)   // 제거될 방이 아니라면
            {
                if (!myRoomList.Contains(roomList[i])) myRoomList.Add(roomList[i]); // 기존에 없던 방이면 새로 추가
                else myRoomList[myRoomList.IndexOf(roomList[i])] = roomList[i];     // 기존에 있던 방이면 정보 갱신
            }
            // 제거해야 될 방인지...
            else if (myRoomList.IndexOf(roomList[i]) != -1) // 기존에 있는 방이라면
                myRoomList.RemoveAt(myRoomList.IndexOf(roomList[i]));
        }

        // 방 목록을 다시 받았을 때 갱신하기 위해 기존에 생성된 RoomNode를 삭제
        for(int i =0; i < roomContentTr.childCount; i++)
        {
            Destroy(roomContentTr.GetChild(i).gameObject);
        }

        // 스크롤 영역 초기화
        roomContentTr.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        for (int i = 0; i < myRoomList.Count; i++)
        {
            //Debug.Log(_room.Name);
            GameObject room = (GameObject)Instantiate(roomNodePrefab);
            // 생성한 RoomItem 프리팝의 Parent를 지정
            room.transform.SetParent(roomContentTr.transform, false);

            // 생성한 RoomItem에 표시하기 위한 텍스트 정보 전달
            RoomNode roomNode = room.GetComponent<RoomNode>();
            roomNode.roomName = myRoomList[i].Name;
            roomNode.userCountText.text = myRoomList[i].PlayerCount + " / " + myRoomList[i].MaxPlayers;;

            // 텍스트 정보를 표시
            roomNode.DispRoomData(myRoomList[i].IsOpen);
            // RoomItem의 Button 컴포넌트에 클릭 이벤트를 동적으로 연결
            // roomData.GetComponent<UnityEngine.UI.Button>().onClick.AddListener( delegate { OnClickRoomItem(roomData.roomName);} );
        }
    }
}