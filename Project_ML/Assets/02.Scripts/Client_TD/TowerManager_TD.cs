using UnityEngine;

// 타워의 체력 관리 및 파괴 여부 감지
public class TowerManager_TD : MonoBehaviour
{
    [Tooltip("타워 최대 체력")]
    public float maxHealth = 1000f;
    [HideInInspector] public float currentHealth;

    // 외부 참조용 (플레이어, 게임매니저 등)
    [HideInInspector] public bool isDestroyed = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }


    // 적이 타워를 공격할 때 데미지 적용
    public void TakeDamage(float dmg, Monster_TD attacker)
    {
        if (isDestroyed) return;

        currentHealth -= dmg;

        Debug.Log($"[Tower] {attacker.name} 에게 {dmg} 데미지 받음, 현재 체력 : {currentHealth:F1}");

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            isDestroyed = true;
            OnTowerDestroyed();
        }
    }

    // 타워 파괴 시 호출 (게임 종료 처리 등)
    private void OnTowerDestroyed()
    {
        Debug.Log("Tower Destroyed!");
    }
}
