// ==========================================================
// SingleGameManager_TD.cs
// 싱글 모드 메인 게임 루프 제어
// - 일시정지 / 카드선택 / 게임오버 관리
// ==========================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SingleGameManager_TD : MonoBehaviour
{
    [Header("게임 상태")]
    public bool isPaused = false;            // 일시정지 여부
    public bool isGameOver = false;          // 게임 종료 여부
    private bool isResting = false;

    [Header("참조")]
    public TowerManager_TD towerManager;     // 타워
    public WaveManager_TD waveManager;       // 웨이브 매니저
    public CardManager_TD cardManager;       // 카드 매니저
    public SkillManager_TD skillManager;     // 스킬 매니저

    [Header("UI (선택)")]
    public GameObject gameOverUI;            // 게임 오버 UI
    public GameObject pauseUI;               // 일시정지 UI

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject skillStatPanel;
    [SerializeField] private float restSeconds = 20f;

    private void Awake()
    {
        if (!towerManager) towerManager = FindObjectOfType<TowerManager_TD>();
        if (!waveManager) waveManager = FindObjectOfType<WaveManager_TD>();
        if (!cardManager) cardManager = FindObjectOfType<CardManager_TD>();
        if (!skillManager) skillManager = FindObjectOfType<SkillManager_TD>();
    }

    private void Update()
    {
        // ESC로 일시정지/해제
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            SetPause(!isPaused);
        }
    }

    // ==========================================================
    // 일시정지 제어
    // ==========================================================
    public void SetPause(bool value)
    {
        isPaused = value;
        Time.timeScale = (isPaused) ? 0f : 1f;

        if (pauseUI)
            pauseUI.SetActive(isPaused);

        Debug.Log($"[SingleGameManager_TD] 일시정지 상태: {isPaused}");
    }

    // ==========================================================
    // 타워 파괴 시 게임 종료 처리
    // ==========================================================
    public void OnTowerDestroyed()
    {
        if (isGameOver) return;

        isGameOver = true;
        SetPause(true);
        Time.timeScale = 0f;

        Debug.Log("[SingleGameManager_TD] 게임 오버 - 타워 파괴됨");

        if (gameOverUI)
            gameOverUI.SetActive(true);
    }

    // ==========================================================
    // 웨이브 종료 후 휴식 페이즈
    // ==========================================================
    public IEnumerator RestPhase()
    {
        isResting = true;

        // 런치타임 들어가자마자 게임은 멈춰서 플레이어는 못 움직이게 하고
        SetPause(true);
        // 보상 선택해야 하니까 커서 보이게
        SetCursor(true);

        // 이때 리워드/스킬 패널 켜기
        if (rewardPanel)
            rewardPanel.SetActive(true);
        if (skillStatPanel)
            skillStatPanel.SetActive(true);

        float remain = restSeconds;

        // 첫 표시
        if (waveText)
            waveText.text = $"Lunch Time\n{Mathf.CeilToInt(remain)}";

        // 타임스케일 0이어도 흘러야 하니까 Realtime으로
        while (remain > 0f)
        {
            yield return new WaitForSecondsRealtime(1f);
            remain -= 1f;

            if (waveText)
                waveText.text = $"Lunch Time\n{Mathf.CeilToInt(remain)}";
        }

        // 런치타임 끝
        if (skillStatPanel)
            skillStatPanel.SetActive(false);

        isResting = false;

        // 여기서 보상 패널이 이미 닫혀 있으면 바로 재개
        if (rewardPanel == null || !rewardPanel.activeSelf)
        {
            SetPause(false);
            SetCursor(false);
        }

        Debug.Log("[SingleGameManager_TD] Rest end");
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
}
