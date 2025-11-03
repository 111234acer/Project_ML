using UnityEngine;


// 싱글 디펜스 모드의 전체 게임 상태 제어
public class SingleGameManager_TD : MonoBehaviour
{
    // 일시정지 여부 (카드 선택, 게임 정지 등)
    [HideInInspector] public bool isPaused = false;

    // 게임이 시작된 시간 (생존 시간 계산용)
    private float gameStartTime;
    private float elapsedTime;

    // TowerManager 참조
    public TowerManager_TD towerManager;

    void Start()
    {
        gameStartTime = Time.time;
    }

    void Update()
    {
        // 타워 파괴 시 → 게임 종료 처리
        if (towerManager && towerManager.isDestroyed)
        {
            OnGameOver();
            return;
        }

        // 생존 시간 갱신
        elapsedTime = Time.time - gameStartTime;
    }

    /// 게임 일시정지 (카드 선택 시 호출 등)
    public void SetPause(bool value)
    {
        isPaused = value;
        Time.timeScale = value ? 0f : 1f;
    }

    // 타워가 파괴되었을 때 게임 종료 처리
    private void OnGameOver()
    {
        Debug.Log($"Game Over! Survived Time: {elapsedTime:F1} seconds");
        SetPause(true);
    }
}
