// ==========================================================
// SingleGameManager_TD.cs
// 싱글 모드 메인 게임 루프 제어
// - 일시정지 / 카드선택 / 게임오버 관리
// ==========================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("UI (선택)")]
    public GameObject gameOverUI;            // 게임 오버 UI
    public GameObject pauseUI;               // 일시정지 UI

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
        Debug.Log("[SingleGameManager_TD] 휴식 페이즈 진입");
        yield return new WaitForSecondsRealtime(2f); // 2초 대기
        Debug.Log("[SingleGameManager_TD] 휴식 종료, 다음 웨이브로 이동");
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
}
