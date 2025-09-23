using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomNode : MonoBehaviour
{
    Button roomNodeBtn;
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI userCountText;

    internal string roomName = "";

    void Start()
    {
        roomNodeBtn = GetComponent<Button>();

        if(roomNodeBtn != null)
        {
            roomNodeBtn.onClick.AddListener(() =>
            {
                // 해당 방으로 입장
                PhotonLobbyManager.instance.OnClickRoomNode(roomName);
            });
        }
    }

    public void DispRoomData(bool a_IsOpen)
    {
        if (a_IsOpen == true)
        {
            roomNameText.color = new Color32(0, 0, 0, 255);
            userCountText.color = new Color32(0, 0, 0, 255);
        }
        else
        {
            roomNameText.color = new Color32(0, 0, 255, 255);
            userCountText.color = new Color32(0, 0, 255, 255);
        }

        roomNameText.text = roomName;
    }
}
