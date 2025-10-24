using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// 캐릭터 선택 씬 전체를 관리하는 핵심 매니저.
/// - 로컬 플레이어의 캐릭터 선택
/// - Photon CustomProperties를 통한 전체 동기화
/// - 모든 인원 선택 완료 시 게임 씬 로드
/// </summary>
public class CharacterSelectManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_Text statusText;                // 하단 상태 텍스트 (“선택 중...”, “루미아 선택됨”)
    public Button confirmButton;               // “선택 확정” 버튼
    public Button backButton;                  // “나가기” 버튼 (로비 복귀용)
    public Transform playerListParent;         // Panel_PlayerList의 VerticalLayoutGroup Transform
    public GameObject playerListEntryPrefab;   // “플레이어1 : 루미아” 형식 텍스트 프리팹

    [Header("Timeout Settings")]
    public float selectionTimeout = 20f;       // 최대 선택 대기 시간

    private Dictionary<int, TMP_Text> playerListEntries = new(); // actorNumber → UI
    private string selectedCharacter = "";
    private bool isConfirmed = false;
    private bool allSelected = false;

    private void Start()
    {
        // 플레이어 리스트 UI 생성
        BuildPlayerList();

        if (statusText != null)
            statusText.text = "캐릭터를 선택하세요.";

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnClickConfirm);

        if (backButton != null)
            backButton.onClick.AddListener(OnClickBack);

        // 선택 제한 타이머
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(TimeoutRoutine());
    }

    // -----------------------------
    // UI → 캐릭터 카드 클릭 시 호출
    // -----------------------------
    public void SelectCharacter(string charName)
    {
        if (isConfirmed) return; // 이미 확정한 상태면 변경 불가

        selectedCharacter = charName;

        if (statusText != null)
            statusText.text = $"선택됨: {charName}";

        // 카드 선택 시각화 전체에 전파
        BroadcastCardSelection(charName);
    }

    // -----------------------------
    // "선택 확정" 버튼
    // -----------------------------
    public void OnClickConfirm()
    {
        if (string.IsNullOrEmpty(selectedCharacter))
        {
            if (statusText != null)
                statusText.text = "먼저 캐릭터를 선택하세요.";
            return;
        }

        isConfirmed = true;

        // Photon CustomProperties에 저장
        Hashtable props = new Hashtable();
        props["SelectedCharacter"] = selectedCharacter;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if (statusText != null)
            statusText.text = $"{selectedCharacter} 선택 확정됨.";

        // 다른 플레이어에게도 알림
        photonView.RPC(nameof(RPC_NotifySelectionChanged), RpcTarget.All);
    }

    // -----------------------------
    // "나가기" 버튼
    // -----------------------------
    public void OnClickBack()
    {
        if (statusText != null)
            statusText.text = "로비로 돌아갑니다...";
        PhotonNetwork.LeaveRoom();
    }

    // -----------------------------
    // 모든 플레이어가 선택 완료되면 게임 시작
    // -----------------------------
    [PunRPC]
    void RPC_NotifySelectionChanged()
    {
        RefreshPlayerListUI();
        CheckAllSelectedAndStart();
    }

    // Photon에서 프로퍼티 바뀔 때마다 호출됨
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        RefreshPlayerListUI();
        CheckAllSelectedAndStart();
    }

    // -----------------------------
    // 전체 선택 완료 감지
    // -----------------------------
    void CheckAllSelectedAndStart()
    {
        bool everyoneSelected = true;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey("SelectedCharacter"))
            {
                everyoneSelected = false;
                break;
            }
        }

        if (everyoneSelected && PhotonNetwork.IsMasterClient && !allSelected)
        {
            allSelected = true;

            if (statusText != null)
                statusText.text = "모든 플레이어 선택 완료! 게임 시작 중...";

            StartCoroutine(LoadGameSceneAfterDelay(2f));
        }
    }

    IEnumerator LoadGameSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LoadLevel("GameScene");
    }

    // -----------------------------
    // 선택 제한 타이머 (마스터 전용)
    // -----------------------------
    IEnumerator TimeoutRoutine()
    {
        yield return new WaitForSeconds(selectionTimeout);

        if (allSelected) yield break;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey("SelectedCharacter"))
            {
                Hashtable props = new Hashtable();
                props["SelectedCharacter"] = "Lumia"; // 기본값
                p.SetCustomProperties(props);
            }
        }

        Debug.Log("[CharacterSelectManager] 선택 제한 시간 초과. 기본 캐릭터로 설정.");
        CheckAllSelectedAndStart();
    }

    // -----------------------------
    // 플레이어 리스트 UI 생성
    // -----------------------------
    void BuildPlayerList()
    {
        if (playerListParent == null || playerListEntryPrefab == null)
            return;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            var entry = Instantiate(playerListEntryPrefab, playerListParent);
            var text = entry.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = $"{p.NickName} : 미선택";
                playerListEntries[p.ActorNumber] = text;
            }
        }
    }

    void RefreshPlayerListUI()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            string selected = p.CustomProperties.ContainsKey("SelectedCharacter")
                ? p.CustomProperties["SelectedCharacter"].ToString()
                : "미선택";

            if (playerListEntries.TryGetValue(p.ActorNumber, out TMP_Text txt) && txt != null)
                txt.text = $"{p.NickName} : {selected}";
        }
    }

    // -----------------------------
    // 카드 하이라이트 반영 (UI용)
    // -----------------------------
    public void BroadcastCardSelection(string selectedId)
    {
        var cards = FindObjectsOfType<CharacterCard>(true);
        foreach (var c in cards)
            c.SetSelected(c.characterId == selectedId);
    }

    // -----------------------------
    // Photon 콜백
    // -----------------------------
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayerListUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayerListUI();
    }
}
