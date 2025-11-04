using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 싱글 디펜스 모드의 전체 게임 상태 제어
public class SingleGameManager_TD : MonoBehaviour
{
    // 일시정지 여부 (카드 선택, 게임 정지 등)
    [HideInInspector] public bool isPaused = false;

    public float restSeconds = 5f;

    // TowerManager 참조
    public TowerManager_TD towerManager;

    void Start()
    {
    }

    void Update()
    {
        // 타워 파괴 시 → 게임 종료 처리
        if (towerManager && towerManager.isDestroyed)
        {
            OnGameOver();
            return;
        }
    }

    /// 게임 일시정지 (카드 선택 시 호출 등)
    public void SetPause(bool value)
    {
        isPaused = value;
    }
        
    private void OnGameOver()
    {
        SetPause(true);
    }

    public IEnumerator RestPhase()
    {
        isPaused = true;
        // 여기서 UI 띄우거나 "다음 웨이브까지 xx초" 이런거 하면 됨
        yield return new WaitForSeconds(restSeconds);
        isPaused = false;
    }
}
