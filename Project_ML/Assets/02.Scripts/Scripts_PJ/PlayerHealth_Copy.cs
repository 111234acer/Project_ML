using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PlayerHealth_Copy : MonoBehaviour
{
    [SerializeField] CharacterController characterController;   // CC만 사용
    [SerializeField] Collider[] deathToggleColliders;           // 비우면 자동 수집
    Dictionary<Collider, bool> _colBackup;
    PhotonView pv;

    [Header("Character Info")]
    public string characterName = "";

    [Header("Character Stats")]
    public int MaxHealth;               // 최대 체력
    public int attackDamage;            // 공격력
    public int currentHealth;           // 현재 체력
    public bool isDead = false;         // 사망 여부

    [Header("Respawn Settings")]
    public float respawnDelay = 8f;     // 리스폰 시간

    [Header("Invincible Settings")]
    public float invincibleTime = 0.2f;         // 피격 직후 무적 시간 
    public float respawnInvincibleTime = 2f;    // 리스폰 후 무적 유지 시간
    private bool isInvincible = false;          // 현재 무적 상태 여부

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (characterController == null) 
            characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        currentHealth = MaxHealth;       
    }

    [PunRPC]
    public void TakeDamage(int damage) // 데미지 함수
    {
        if (isDead || isInvincible) return;                              // 죽으면 데미지 무시

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        // 피격 직후 짧은 무적 프레임 적용(중복 피격 방지)
        StartCoroutine(InvincibleCoroutine(invincibleTime));

        if (currentHealth <= 0) // 체력이 0이되면 Die 함수 실행
        {
            Die();
        }
    }

    public void Heal(int amount) // 체력을 회복하는 함수
    {
        if (isDead) return;          // 죽으면 힐 불가능
        pv.RPC("RPC_Heal", RpcTarget.All, amount);
    }

    [PunRPC]
    void RPC_Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        isInvincible = true;        // 무적 시간
        yield return new WaitForSeconds(duration);  // duration초 기다림
        isInvincible = false;       // 무적 해제
    }

    void Die()  // 사망 처리 함수
    {
        if (isDead) return;      // 이미 죽어있으면 중복 실행 방지
        isDead = true;

        SetPassThrough(true);

        var gm = FindObjectOfType<GameManager>();
        if (gm != null && pv != null)
            gm.RequestRespawn(pv, respawnDelay);
    }

    // 리스폰 처리
    [PunRPC]
    public void RPC_RespawnAt(Vector3 pos, Vector3 forward, int hp, float invuln = 1.0f)
    {
        // 좌표/회전
        transform.SetPositionAndRotation(pos, Quaternion.LookRotation(forward, Vector3.up));

        // 체력/상태 리셋
        currentHealth = Mathf.Clamp(hp, 1, MaxHealth);
        isDead = false;

        // 리스폰 무적
        SetPassThrough(false);
        StartCoroutine(InvincibleCoroutine(Mathf.Max(invuln, respawnInvincibleTime)));
    }

    void SetPassThrough(bool enable)
    {
        var cols = (deathToggleColliders != null && deathToggleColliders.Length > 0)
            ? deathToggleColliders
            : GetComponentsInChildren<Collider>(true);

        if (enable)
        {
            if (_colBackup == null) _colBackup = new Dictionary<Collider, bool>();
            _colBackup.Clear();
            foreach (var c in cols)
            {
                if (!c) continue;
                _colBackup[c] = c.isTrigger;
                c.isTrigger = true;            // 전부 통과 가능
            }
            if (characterController) characterController.enabled = false; // CC 충돌 끔
        }
        else
        {
            if (_colBackup != null)
            {
                foreach (var kv in _colBackup) if (kv.Key) kv.Key.isTrigger = kv.Value;
                _colBackup.Clear();
            }
            if (characterController) characterController.enabled = true;  // CC 복구
        }
    }

    // 현재 체력 비율 (UI용)
    public float GetHealthPercent()
    {
        return (float)currentHealth / MaxHealth;
    }

    // 공격력 가져오기 (공격할 때 사용)
    public int GetAttackDamage()
    {
        return attackDamage;
    }

    public bool IsDead()
    {
        return isDead;
    }
}