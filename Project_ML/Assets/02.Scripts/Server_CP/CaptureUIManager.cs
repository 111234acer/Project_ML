using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptureUIManager : MonoBehaviour
{
    public TMP_Text redPercentText;
    public TMP_Text bluePercentText;
    public TMP_Text ownerTeamText;
    public GameObject lockIcon;
    public Slider flipProgressBar;
    public TMP_Text flipProgressLabel;
    public TMP_Text overtimeText;
    public TMP_Text gameTimeText;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public Slider overtimeBar;
    public Slider redFillSlider;
    public Slider blueFillSlider;

    public float barLerpSpeed = 20f;

    float displayOvertime = 0f;

    private CapturePointManager cap;


    public void Init(CapturePointManager capturePoint)
    {
        cap = capturePoint;
    }

    void Update()
    {
        if (cap == null) return;

        if (redPercentText != null) redPercentText.text = Mathf.RoundToInt(cap.RedScore) + "%";
        if (bluePercentText != null) bluePercentText.text = Mathf.RoundToInt(cap.BlueScore) + "%";

        if (redFillSlider != null)
        {
            redFillSlider.value = Mathf.Clamp01(cap.RedScore / 100f);
        }
        if (blueFillSlider != null)
        {
            blueFillSlider.value = Mathf.Clamp01(cap.BlueScore / 100f);
        }

        if (ownerTeamText != null)
        {
            if (cap.PreLockTimer > 0f) ownerTeamText.text = "";
            else ownerTeamText.text = (cap.OwnerTeam == 0) ? "RED" : (cap.OwnerTeam == 1 ? "BLUE" : "");
        }

        if (lockIcon != null) lockIcon.SetActive(cap.PreLockTimer > 0f);

        bool showFlip = (cap.PreLockTimer <= 0f) && cap.AmInside && cap.myPlayer != null && cap.OwnerTeam != cap.myPlayer.team;

        if (flipProgressBar != null)
        {
            flipProgressBar.gameObject.SetActive(showFlip);
            if (showFlip)
            {
                float denom = (cap.sustainToFlip <= 0f) ? 0.01f : cap.sustainToFlip;
                flipProgressBar.value = Mathf.Clamp01(cap.FlipTimer / denom);
            }
        }
        if (flipProgressLabel != null)
        {
            flipProgressLabel.gameObject.SetActive(showFlip);
            if (showFlip) flipProgressLabel.text = "Á¡·É Áß";
        }

        bool isOvertime = (cap.OvertimeRemain > 0f && cap.OwnerTeam != -1);
        if (overtimeText != null) overtimeText.text = isOvertime ? "OVERTIME" : "";

        if (overtimeBar)
        {
            overtimeBar.gameObject.SetActive(isOvertime);
            overtimeBar.maxValue = Mathf.Max(0.01f, cap.overtimeBonus);

            if (isOvertime)
            {
                if (cap.OvertimeRemain > displayOvertime)
                    displayOvertime = cap.OvertimeRemain;
                else
                    displayOvertime = Mathf.MoveTowards(displayOvertime, cap.OvertimeRemain, barLerpSpeed * Time.deltaTime);

                displayOvertime = Mathf.Clamp(displayOvertime, 0f, overtimeBar.maxValue);
                overtimeBar.value = displayOvertime;
            }
            else
            {
                displayOvertime = 0f;
                overtimeBar.value = 0f;
            }
        }


        if (gameTimeText != null)
        {
            int t = Mathf.FloorToInt(cap.TotalGameTime);
            gameTimeText.text = (t / 60).ToString("00") + ":" + (t % 60).ToString("00");
        }
    }

    public void ShowEndPanel(bool isWin)
    {
        if (isWin)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }
        else
        {
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }
    }
}
