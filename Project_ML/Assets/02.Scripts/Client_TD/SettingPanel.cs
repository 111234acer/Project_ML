using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingPanel : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject fadePanel;

    public AudioMixer mixer;
    public Slider soundSlider;
    public Slider sensitivitySlider;
    public string exposedSFXParam = "SFXVolume";

    private SingleGameManager_TD singleGameManager;
    private const string KEY_SFX_VOLUME = "SFXVolumeLinear";

    private const string KEY_MOUSE_SENS = "TD_MouseSensitivity";
    private const float DEFAULT_SENS = 200f;

    private bool isOpen = false;
    private bool wasPausedByGame = false;

    private PlayerSFX_TD playerSFX;
    private PlayerLook_TD playerLook;

    private void Awake()
    {
        singleGameManager = FindObjectOfType<SingleGameManager_TD>();

        if (settingPanel) settingPanel.SetActive(false);
        if (fadePanel) fadePanel.SetActive(false);
    }

    private void Start()
    {
        float saved = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
        ApplySFXVolume(saved);

        if (soundSlider)
        {
            soundSlider.value = saved;
            soundSlider.onValueChanged.AddListener(OnSoundSliderChanged);
        }

        float savedSens = PlayerPrefs.GetFloat(KEY_MOUSE_SENS, DEFAULT_SENS);
        if (sensitivitySlider)
        {
            sensitivitySlider.value = savedSens;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isOpen)
            {
                CloseSetting();
            }
            else
            {
                if (singleGameManager != null && singleGameManager.isPaused)
                {
                    return;
                }

                OpenSetting();
            }
        }
    }

    public void OpenSetting()
    {
        isOpen = true;

        settingPanel.SetActive(true);
        fadePanel.SetActive(true);

        if (singleGameManager != null)
        {
            wasPausedByGame = singleGameManager.isPaused;
            singleGameManager.SetPause(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!playerSFX) playerSFX = FindObjectOfType<PlayerSFX_TD>();
        if (playerSFX) playerSFX.PauseAll();

        if (!playerLook) playerLook = FindObjectOfType<PlayerLook_TD>();
    }

    public void CloseSetting()
    {
        isOpen = false;

        settingPanel.SetActive(false);
        fadePanel.SetActive(false);

        if (singleGameManager != null)
        {
            if (!wasPausedByGame)
            {
                singleGameManager.SetPause(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (!playerSFX) playerSFX = FindObjectOfType<PlayerSFX_TD>();
                if (playerSFX) playerSFX.ResumeAll();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void OnSoundSliderChanged(float linear)
    {
        ApplySFXVolume(linear);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, linear);
        PlayerPrefs.Save();
    }

    private void ApplySFXVolume(float linear)
    {
        if (!mixer) return;

        float clamped = Mathf.Clamp(linear, 0.0001f, 1f);
        float dB = Mathf.Log10(clamped) * 20f;

        mixer.SetFloat(exposedSFXParam, dB);
    }

    private void OnSensitivityChanged(float value)
    {
        ApplySensitivity(value);
        PlayerPrefs.SetFloat(KEY_MOUSE_SENS, value);
        PlayerPrefs.Save();
    }

    private void ApplySensitivity(float value)
    {
        if(!playerLook) playerLook = FindObjectOfType<PlayerLook_TD>();

        if(playerLook)
        {
            playerLook.SetMouseSensitivity(value, false);
        }
    }
}
