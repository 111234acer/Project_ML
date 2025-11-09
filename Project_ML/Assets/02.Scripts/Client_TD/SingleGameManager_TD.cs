// ==========================================================
// SingleGameManager_TD.cs
// 싱글 모드 메인 게임 루프 제어
// - 일시정지 / 카드선택 / 게임오버 관리
// ==========================================================
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SingleGameManager_TD : MonoBehaviour
{
    [Header("게임 상태")]
    public bool isPaused = false;            // 일시정지 여부
    public bool isGameOver = false;          // 게임 종료 여부

    [Header("참조")]
    public TowerManager_TD towerManager;     // 타워
    public WaveManager_TD waveManager;       // 웨이브 매니저
    public CardManager_TD cardManager;       // 카드 매니저
    public SkillManager_TD skillManager;     // 스킬 매니저

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private GameObject rewardPanel;
    //[SerializeField] private GameObject skillStatPanel;
    [SerializeField] private float restSeconds = 20f;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private GameObject fadePanel;
    [SerializeField] private GameObject failurePanel;
    [SerializeField] private GameObject recordPanel;
    [SerializeField] private TextMeshProUGUI finalWaveText;
    [SerializeField] private TextMeshProUGUI recordText;
    [SerializeField] private string mainSceneName = "MainScene";

    private GameObject playerInstance;
    private int currentWaveIndex = 0;
    private const string KEY_WAVE_RECORDS = "TD_WAVE_RECORDS";

    private void Awake()
    {
        if (!towerManager) towerManager = FindObjectOfType<TowerManager_TD>();
        if (!waveManager) waveManager = FindObjectOfType<WaveManager_TD>();
        if (!cardManager) cardManager = FindObjectOfType<CardManager_TD>();
        if (!skillManager) skillManager = FindObjectOfType<SkillManager_TD>();
    }

    private void Start()
    {
        SpawnPlayer();

        if (fadePanel) fadePanel.SetActive(false);
        if (failurePanel) failurePanel.SetActive(false);
        if (recordPanel) recordPanel.SetActive(false);
    }

    private void SpawnPlayer()
    {
        if (CharacterSelectManager_TD.SelectedCharacterPrefab != null)
        {
            playerInstance = Instantiate(CharacterSelectManager_TD.SelectedCharacterPrefab,
                spawnPoint.position, spawnPoint.rotation);
        }
    }

    // ==========================================================
    // 일시정지 제어
    // ==========================================================
    public void SetPause(bool value)
    {
        isPaused = value;
        Time.timeScale = (isPaused) ? 0f : 1f;
    }

    // ==========================================================
    // 타워 파괴 시 게임 종료 처리
    // ==========================================================
    public void OnTowerDestroyed()
    {
        if (isGameOver) return;

        isGameOver = true;

        StartCoroutine(GameOverFlow());
    }

    private IEnumerator GameOverFlow()
    {
        SetPause(true);
        Time.timeScale = 0f;
        SetCursor(true);

        failurePanel.SetActive(true);
        fadePanel.SetActive(true);

        SaveWaveRecord(currentWaveIndex);

        yield return new WaitForSecondsRealtime(2f);

        if (recordPanel)
        {
            UpdateRecordPanelUI(currentWaveIndex);
            failurePanel.SetActive(false);
            recordPanel.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(3f);

        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainSceneName))
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }
    // ==========================================================
    // 웨이브 종료 후 휴식 페이즈
    // ==========================================================
    public IEnumerator RestPhase()
    {
        SetPause(true);
        SetCursor(true);

        if (rewardPanel)
            rewardPanel.SetActive(true);

        float remain = restSeconds;
        bool resumedAfterReward = false;

        if (waveText)
            waveText.text = $"Lunch Time\n{Mathf.CeilToInt(remain)}";

        while (remain > 0f)
        {
            if (rewardPanel != null && rewardPanel.activeSelf)
            {
                yield return new WaitForSecondsRealtime(1f);
            }
            else
            {
                if (!resumedAfterReward)
                {
                    SetPause(false);
                    SetCursor(false);
                    resumedAfterReward = true;
                }

                yield return new WaitForSeconds(1f);
            }

            remain -= 1f;

            if (waveText)
                waveText.text = $"Lunch Time\n{Mathf.CeilToInt(remain)}";
        }

        if (rewardPanel.activeSelf)
        {
            rewardPanel.SetActive(false);
            SetPause(false);
            SetCursor(false);
        }
    }

    // ==========================================================
    // 재시작 / 메인 메뉴 복귀
    // ==========================================================
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowWaveText(int waveIndex)
    {
        if (waveText)
            waveText.text = $"Wave\n{waveIndex}";

        currentWaveIndex = waveIndex;
    }

    public void OnClickRewardButton()
    {
        if (rewardPanel)
            rewardPanel.SetActive(false);

        SetPause(false);
        SetCursor(false);
    }

    void SetCursor(bool show)
    {
        if (show)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SaveWaveRecord(int wave)
    {
        List<int> list = LoadWaveRecords();

        list.Add(wave);
        list.Sort((a, b) => b.CompareTo(a));

        if (list.Count > 5)
            list = list.GetRange(0, 5);

        string save = string.Join(",", list);
        PlayerPrefs.SetString(KEY_WAVE_RECORDS, save);
        PlayerPrefs.Save();
    }

    private List<int> LoadWaveRecords()
    {
        List<int> list = new List<int>();
        string saved = PlayerPrefs.GetString(KEY_WAVE_RECORDS, string.Empty);
        if (string.IsNullOrEmpty(saved))
            return list;

        string[] tokens = saved.Split(',');
        foreach (var t in tokens)
        {
            if (int.TryParse(t, out int v))
                list.Add(v);
        }
        return list;
    }

    private void UpdateRecordPanelUI(int finalWave)
    {
        if (finalWaveText)
            finalWaveText.text = $"Final Wave: {finalWave}";

        List<int> list = LoadWaveRecords();

        if (recordText)
        {
            if (list.Count == 0)
            {
                recordText.text = string.Empty;
            }
            else
            {
                int count = Mathf.Min(5, list.Count);
                System.Text.StringBuilder sb = new System.Text.StringBuilder(64);
                for (int i = 0; i < count; i++)
                {
                    if (i > 0) sb.Append('\n');
                    sb.Append(i + 1).Append("th ")
                      .Append(list[i]).Append(" wave");
                }
                recordText.text = sb.ToString();
            }
        }
    }
}
