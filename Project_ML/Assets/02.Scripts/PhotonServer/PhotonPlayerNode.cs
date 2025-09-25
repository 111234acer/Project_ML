using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PhotonPlayerNode : MonoBehaviour
{
    public TextMeshProUGUI nicknameText;
    public GameObject readyObj;
    public GameObject hostObj;
    
    internal int uniqueId = -1;
    internal string team = "";
    internal bool isReady = false;
    internal string nickname = "";
    internal bool isHost = false;

    RectTransform rect;
    void Start()
    {
        rect = GetComponent<RectTransform>();
        rect.localScale = new Vector3(1, 1, 1);
        RefreshPlayerState();
    }

    public void RefreshPlayerState()
    {
        nicknameText.text = nickname;
        if (isReady == true)
            readyObj.SetActive(true);
        else
            readyObj.SetActive(false);

        if (isHost)
            hostObj.SetActive(true);
        else
            hostObj.SetActive(false);
    }
}
