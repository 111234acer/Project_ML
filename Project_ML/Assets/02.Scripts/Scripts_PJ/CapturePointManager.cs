using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CapturePointManager : MonoBehaviourPunCallbacks
{
    [Header("거점 기본설정")]
    public string pointName = "A";
    public float captureTime = 10f;

    [Header("UI 설정")]
    public Slider redSlider;
    public Slider blueSlider;

    private int redCount = 0;
    private int blueCount = 0;
    private int controllingTeam = -1;

    private float captureProgressRed = 0f;
    private float captureProgressBlue = 0f;

    void Start()
    {
        if (redSlider != null) redSlider.value = 0f;
        if (blueSlider != null) blueSlider.value = 0f;
    }

    void Update()
    {
        // 마스터 클라이언트만 점령 계산 담당
        if (PhotonNetwork.IsMasterClient)
        {
            HandleCaptureLogic();
            // 모든 클라이언트에게 현재 진행도를 동기화
            photonView.RPC(nameof(SyncCaptureUI), RpcTarget.All,
                captureProgressRed, captureProgressBlue, controllingTeam);
        }
    }

    private void HandleCaptureLogic()
    {
        // 특정 팀만 있을 때 점령 진행
        if (redCount > 0 && blueCount == 0)
        {
            controllingTeam = 0;
            captureProgressRed += Time.deltaTime / captureTime;
        }
        else if (blueCount > 0 && redCount == 0)
        {
            controllingTeam = 1;
            captureProgressBlue += Time.deltaTime / captureTime;
        }

        // 0~1로 Clamp
        captureProgressRed = Mathf.Clamp01(captureProgressRed);
        captureProgressBlue = Mathf.Clamp01(captureProgressBlue);
    }

    [PunRPC]
    void SyncCaptureUI(float red, float blue, int team)
    {
        captureProgressRed = red;
        captureProgressBlue = blue;
        controllingTeam = team;

        if (redSlider != null) redSlider.value = captureProgressRed;
        if (blueSlider != null) blueSlider.value = captureProgressBlue;
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        if (player.team == 0) redCount++;
        else if (player.team == 1) blueCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        if (player.team == 0) redCount = Mathf.Max(0, redCount - 1);
        else if (player.team == 1) blueCount = Mathf.Max(0, blueCount - 1);
    }
}
