using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class PhotonLobbyManager : MonoBehaviourPunCallbacks
{
    public static PhotonLobbyManager instance; // 싱글톤 인스턴스
    
    string localPlayerNickname = ""; // 유저의 닉네임을 설정합니다.
    string nextSceneName = "PhotonInGame";
    public Button randomMatchingBtn; // 랜덤 매칭 버튼
    public TextMeshProUGUI loadingText;        // 로딩 텍스트

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
            Destroy(this);

        //포톤 클라우드 서버 접속 여부 확인(인게임에서 빠져나온 경우가 있기 때문에...)
        if (!PhotonNetwork.IsConnected)
        {
            //1번, 포톤 클라우드에 접속
            PhotonNetwork.ConnectUsingSettings();
            //포톤 서버에 접속시도(지역 서버 접속) -> 사용자 인증 -> 로비 입장 진행
        }
    }

    void Start()
    {
        if(randomMatchingBtn != null)
        {
            randomMatchingBtn.onClick.AddListener(() =>
            {
                ClickJoinRandomRoom();
            });
        }
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

        //3번, 규모가 작은 게임에서는 로비가 보통 하나이고...
        PhotonNetwork.JoinLobby();
        //대형 게임인 경우 상급자로비, 중급자로비, 초보자로비 처럼 
        //로비가 여러개일 수 있다. 
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

        // 로비 접속이 완료되어야 버튼 활성화
        // 로비화면 플레이어 캐릭터 셋팅
    }

    //--------------------------------------------------------------------------------------- 방 접속

    public void ClickJoinRandomRoom()         //3번 방 입장 요청 버튼 누름
    {
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = localPlayerNickname;
        //플레이어 이름을 저장

        //5번 무작위로 추출된 방으로 입장
        PhotonNetwork.JoinRandomRoom();
    }

    //PhotonNetwork.JoinRandomRoom() 이 함수 실패한 경우 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 방 참가 실패 (참가할 방이 존재하지 않습니다.)");

        //룸 생성
        PhotonNetwork.CreateRoom("MyRoom");
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
        StartCoroutine(this.LoadGameScene());
    }

    //(같은 이름의 방이 있을 때 실패함)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 만들기 실패");
        //주로 같은 이름의 방이 존재할 때 룸생성 에러가 발생된다.
        Debug.Log(returnCode.ToString()); //오류 코드(ErrorCode 클래스)
        Debug.Log(message); //오류 메시지
    }

    //룸 씬으로 이동하는 코루틴 함수
    IEnumerator LoadGameScene() // 최종 게임 씬 로딩 --> 6번 or 5번
    {
        //씬을 이동하는 동안 포톤 클라우드 서버로부터 네트워크 메시지 수신 중단
        PhotonNetwork.IsMessageQueueRunning = false;
        //백그라운드로 씬 로딩

        Time.timeScale = 1.0f;  //게임에 들어갈 때는 원래 속도로...

        AsyncOperation ao =
          UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(nextSceneName);

        yield return ao;
    }
    #endregion
}
