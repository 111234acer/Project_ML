using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class PhotonLobbyManager : MonoBehaviourPunCallbacks
{
    public static PhotonLobbyManager instance; // �̱��� �ν��Ͻ�
    
    string localPlayerNickname = ""; // ������ �г����� �����մϴ�.
    string nextSceneName = "PhotonInGame";
    public Button randomMatchingBtn; // ���� ��Ī ��ư
    public TextMeshProUGUI loadingText;        // �ε� �ؽ�Ʈ

    public static PhotonLobbyManager Instance // �̱��� ������Ƽ
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

        //���� Ŭ���� ���� ���� ���� Ȯ��(�ΰ��ӿ��� �������� ��찡 �ֱ� ������...)
        if (!PhotonNetwork.IsConnected)
        {
            //1��, ���� Ŭ���忡 ����
            PhotonNetwork.ConnectUsingSettings();
            //���� ������ ���ӽõ�(���� ���� ����) -> ����� ���� -> �κ� ���� ����
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


    #region ���� �κ� ����
    //-------------------------------------------------------------------------------------- �κ�����

    //2��, ConnectUsingSettings() �Լ� ȣ�⿡ ���� ���� ������ �����ϸ� ȣ��Ǵ� �ݹ� �Լ�
    //PhotonNetwork.LeaveRoom(); ���� ���� ���� ���� �� �Լ��� �ڵ����� ȣ��ȴ�.
    public override void OnConnectedToMaster()
    {   //���⼭ Master�� ������ ���� ������ �ǹ��Ѵ�.
        Debug.Log("���� ���� �Ϸ�");
        //�ܼ� ���� ���� ���Ӹ� �� ���� (ConnectedToMaster)

        //3��, �Ը� ���� ���ӿ����� �κ� ���� �ϳ��̰�...
        PhotonNetwork.JoinLobby();
        //���� ������ ��� ����ڷκ�, �߱��ڷκ�, �ʺ��ڷκ� ó�� 
        //�κ� �������� �� �ִ�. 
    }

    //4��, PhotonNetwork.JoinLobby() ������ ȣ��Ǵ� �κ� ���� �ݹ��Լ�
    public override void OnJoinedLobby()
    {
        Debug.Log("�κ����ӿϷ�");

        //�������� ����� ������ ����
        //ExitGames.Client.Photon.Hashtable roomProperties =
        //new ExitGames.Client.Photon.Hashtable() { { "map", 1 }, { "minLevel", 10 } };
        //PhotonNetwork.JoinRandomRoom(roomProperties, 4);
        //PhotonNetwork.JoinRandomRoom();

        // �κ� ������ �Ϸ�Ǿ�� ��ư Ȱ��ȭ
        // �κ�ȭ�� �÷��̾� ĳ���� ����
    }

    //--------------------------------------------------------------------------------------- �� ����

    public void ClickJoinRandomRoom()         //3�� �� ���� ��û ��ư ����
    {
        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = localPlayerNickname;
        //�÷��̾� �̸��� ����

        //5�� �������� ����� ������ ����
        PhotonNetwork.JoinRandomRoom();
    }

    //PhotonNetwork.JoinRandomRoom() �� �Լ� ������ ��� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("���� �� ���� ���� (������ ���� �������� �ʽ��ϴ�.)");

        //�� ����
        PhotonNetwork.CreateRoom("MyRoom");
        // ���� ���� ���� ���� ���� ����� ������ ������.
        // ( 5�� ���� �α��� �ÿ� ���� ������ �ϰ� �� Client�� �������� ������ �� ���̴�.)
    }

    //PhotonNetwork.CreateRoom() �� �Լ��� �����ϸ� 2��°�� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRoom() �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRandomRoom(); �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    public override void OnJoinedRoom()
    {
        // ���������� ���         [6�� : ������]
        // Ŭ���̾�Ʈ ������ ���  [5�� : ������]
        Debug.Log("�� ���� �Ϸ�");

        //�� ������ �̵��ϴ� �ڷ�ƾ ����
        StartCoroutine(this.LoadGameScene());
    }

    //(���� �̸��� ���� ���� �� ������)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("�� ����� ����");
        //�ַ� ���� �̸��� ���� ������ �� ����� ������ �߻��ȴ�.
        Debug.Log(returnCode.ToString()); //���� �ڵ�(ErrorCode Ŭ����)
        Debug.Log(message); //���� �޽���
    }

    //�� ������ �̵��ϴ� �ڷ�ƾ �Լ�
    IEnumerator LoadGameScene() // ���� ���� �� �ε� --> 6�� or 5��
    {
        //���� �̵��ϴ� ���� ���� Ŭ���� �����κ��� ��Ʈ��ũ �޽��� ���� �ߴ�
        PhotonNetwork.IsMessageQueueRunning = false;
        //��׶���� �� �ε�

        Time.timeScale = 1.0f;  //���ӿ� �� ���� ���� �ӵ���...

        AsyncOperation ao =
          UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(nextSceneName);

        yield return ao;
    }
    #endregion
}
