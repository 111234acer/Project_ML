using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI charNameText;     // 우측상단 이름
    public TextMeshProUGUI jobText;          // 우측상단 직업
    public Image[] skillIcons;               // 아래 3개
    public Image centerPortrait;             // 중앙 큰 이미지
    public Button confirmBtn;

    [Header("Cards")]
    public CharacterCard[] cards;          // 9칸(딜/힐/탱 각 3개, 더미는 interactable=false)

    [Header("Config")]
    public float selectDuration = 25f;       // 선택 제한시간(서버 기준)

    const string RP_PickMask = "PickMask";
    const string RP_EndAt = "PickEndAt";
    const string PK_Char = "Char";

    PhotonView pv;
    int localSelected = -1;

    bool _portraitActivated = false;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;

        if (centerPortrait) centerPortrait.gameObject.SetActive(false);

        // 카드 초기화 연결
        for (int i = 0; i < cards.Length; i++)
            cards[i].Init(this, i);

        confirmBtn.onClick.AddListener(OnClickConfirm);

        // 마스터가 룸 상태 초기화
        if (PhotonNetwork.IsMasterClient)
        {
            var rp = new Hashtable
            {
                [RP_PickMask] = 0,
                [RP_EndAt] = PhotonNetwork.Time + selectDuration
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(rp);
        }

        // 내 Char 기본값 보장(-1)
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(PK_Char))
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { [PK_Char] = -1 });

        UpdateAllUI();
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;

        double endAt = GetRoomDouble(RP_EndAt, 0);
        if (endAt <= 0) return;

        double remain = Math.Max(0, endAt - PhotonNetwork.Time);
        if (timerText) timerText.text = Math.Ceiling(remain).ToString("0");

        // 마스터만, 조건 충족 시 '한 번만' 로드: 호출 후 Update 중단
        if (PhotonNetwork.IsMasterClient && (remain <= 0 || AllLocked()))
        {
            enabled = false;                  // ← 로드 완료 전까지 재호출 차단(가드변수 없음)
            PhotonNetwork.LoadLevel("Scene_ClientTest"); // 너의 인게임 씬명
        }
    }

    // ---------- 선택 UI ----------
    public void HoverSelect(int charId)
    {
        if (GetTaken(charId)) return;
        localSelected = charId;
        PaintPreview(charId);
        confirmBtn.interactable = true;
    }

    void OnClickConfirm()
    {
        if (localSelected < 0) return;
        pv.RPC(nameof(Server_TryLockPick), RpcTarget.MasterClient, localSelected);
    }

    // ---------- 서버권위 ----------
    [PunRPC]
    void Server_TryLockPick(int charId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 시간 끝났으면 리젝트
        double endAt = GetRoomDouble(RP_EndAt, 0);
        if (PhotonNetwork.Time > endAt) { TargetDeny(info.Sender); return; }

        // 이미 누가 선점?
        int mask = GetRoomInt(RP_PickMask, 0);
        bool taken = ((mask >> charId) & 1) == 1;

        // 신청자 이미 다른 캐릭 가졌는지
        int cur = GetPlayerInt(info.Sender, PK_Char, -1);
        if (cur >= 0) { TargetDeny(info.Sender); return; }

        if (taken) { TargetDeny(info.Sender); return; }

        // 확정
        mask |= (1 << charId);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { [RP_PickMask] = mask });
        info.Sender.SetCustomProperties(new Hashtable { [PK_Char] = charId });

        // 모두에게 UI 반영 지시(버퍼드)
        pv.RPC(nameof(Client_OnPickLocked), RpcTarget.AllBuffered, info.Sender.ActorNumber, charId);
    }

    void TargetDeny(Player p)
    {
        pv.RPC(nameof(Client_OnPickDenied), p);
    }

    [PunRPC]
    void Client_OnPickDenied()
    {
        // 실패 알림(사운드/토스트 등)만. 선택 해제
        confirmBtn.interactable = false;
    }

    [PunRPC]
    void Client_OnPickLocked(int actorNumber, int charId)
    {
        // 슬롯 잠금 반영
        for (int i = 0; i < cards.Length; i++)
            cards[i].SetTaken(GetTaken(i));

        // 본인이라면 미리보기 잠금
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            confirmBtn.interactable = false;
        }
    }

    // ---------- Photon 콜백 ----------
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RP_PickMask))
            for (int i = 0; i < cards.Length; i++)
                cards[i].SetTaken(GetTaken(i));
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PK_Char))
            for (int i = 0; i < cards.Length; i++)
                cards[i].SetTaken(GetTaken(i));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 마스터가 떠난 사람의 픽을 해제해 줌(자원 회수)
        if (!PhotonNetwork.IsMasterClient) return;
        int c = GetPlayerInt(otherPlayer, PK_Char, -1);
        if (c < 0) return;
        int mask = GetRoomInt(RP_PickMask, 0);
        mask &= ~(1 << c);
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { [RP_PickMask] = mask });
    }

    // ---------- 헬퍼 ----------
    bool AllLocked()
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (GetPlayerInt(p, PK_Char, -1) < 0) return false;
        return true;
    }

    bool GetTaken(int charId)
    {
        int mask = GetRoomInt(RP_PickMask, 0);
        return ((mask >> charId) & 1) == 1;
    }

    int GetRoomInt(string k, int defV) =>
        (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(k, out var v) && v is int i) ? i : defV;
    double GetRoomDouble(string k, double defV) =>
        (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(k, out var v) && v is double d) ? d : defV;
    int GetPlayerInt(Player p, string k, int defV) =>
        (p != null && p.CustomProperties != null && p.CustomProperties.TryGetValue(k, out var v) && v is int i) ? i : defV;

    void UpdateAllUI()
    {
        for (int i = 0; i < cards.Length; i++)
            cards[i].SetTaken(GetTaken(i));
        PaintPreview(-1);
        confirmBtn.interactable = false;
    }

    public void PaintPreview(int charId)
    {
        var data = CharacterCatalog.Instance.Get(charId);
        if (data == null)
        {
            if (!_portraitActivated)
            {
                if (charNameText) charNameText.text = "";
                if (jobText) jobText.text = "";
                if (centerPortrait) centerPortrait.sprite = null;
                for (int i = 0; i < skillIcons.Length; i++)
                    if (skillIcons[i]) skillIcons[i].sprite = null;
            }
            return;
        }

        if (charNameText) charNameText.text = data.displayName;
        if (jobText) jobText.text = data.role.ToString(); // Dealer/Healer/Tanker
        if (centerPortrait) centerPortrait.sprite = data.portrait;
        for (int i = 0; i < skillIcons.Length; i++)
            if (i < data.skillIcons.Count && skillIcons[i]) skillIcons[i].sprite = data.skillIcons[i];

        if (!_portraitActivated && centerPortrait)
        {
            centerPortrait.gameObject.SetActive(true);
            _portraitActivated = true;
        }
    }
}
