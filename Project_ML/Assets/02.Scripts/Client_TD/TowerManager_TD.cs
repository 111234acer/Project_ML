// ==========================================================
// TowerManager_TD.cs
// 타워 체력 관리 및 파괴 처리
// ==========================================================
using UnityEngine;
using UnityEngine.UI;

public class TowerManager_TD : MonoBehaviour
{
    [Header("타워 기본 설정")]
    [Tooltip("타워 최대 체력")]
    public float maxHealth = 1000f;
    [Tooltip("현재 체력 (런타임에서 변경됨)")]
    public float currentHealth;
    [Tooltip("파괴 여부")]
    public bool isDestroyed = false;

    public Slider hpBar;
    public Canvas hpCanvas;

    [Header("참조")]
    public SingleGameManager_TD singleGameManager; // 게임 루프 제어 매니저
    public Animator towerAnimator;   // 타워 파괴 애니메이션 (선택)
    public AudioSource towerAudio;   // 타워 파괴 사운드 (선택)

    private void Awake()
    {
        currentHealth = maxHealth;

        if (!singleGameManager)
            singleGameManager = FindObjectOfType<SingleGameManager_TD>();

        if (hpBar)
            hpBar.maxValue = maxHealth;

        UpdateHPUI();
    }

    // ==========================================================
    // 데미지 및 회복 처리
    // ==========================================================

    /// <summary>
    /// 몬스터 공격 등으로 데미지를 입었을 때 호출
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDestroyed) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHPUI();

        if (currentHealth <= 0f)
        {
            OnTowerDestroyed();
        }
    }

    /// <summary>
    /// 카드나 스킬 등으로 타워 체력을 회복할 때 호출
    /// </summary>
    public void Recover(float healAmount)
    {
        if (isDestroyed) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHPUI();

        Debug.Log($"[TowerManager_TD] 회복됨 +{healAmount} (현재 {currentHealth}/{maxHealth})");
    }

    // ==========================================================
    // 파괴 처리
    // ==========================================================

    private void OnTowerDestroyed()
    {
        isDestroyed = true;
        currentHealth = 0f;
        UpdateHPUI();

        Debug.Log("[TowerManager_TD] 타워 파괴됨! 게임 종료");

        if (towerAnimator)
            towerAnimator.SetTrigger("Destroyed");

        if (towerAudio)
            towerAudio.Play();

        // ? 게임 매니저에 알림
        if (singleGameManager)
            singleGameManager.OnTowerDestroyed();
    }

    // ==========================================================
    // HP UI 갱신
    // ==========================================================
    private void UpdateHPUI()
    {
        if (hpBar)
            hpBar.value = currentHealth;

        if (hpCanvas)
            hpCanvas.enabled = !isDestroyed;
    }

#if UNITY_EDITOR
    private void Update()
    {
        // 테스트용 키
        if (Input.GetKeyDown(KeyCode.D))
            TakeDamage(50);
        if (Input.GetKeyDown(KeyCode.H))
            Recover(50);
    }
#endif
}
