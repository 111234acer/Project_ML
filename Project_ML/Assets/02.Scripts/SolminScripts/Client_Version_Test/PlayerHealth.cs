using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("Character Info")]
    public string characterName = "";

    [Header("Character Stats")]
    public int MaxHealth;               // 최대 체력
    public int attackDamage;            // 공격력
    public int currentHealth;           // 현재 체력
    public bool isDead = false;         // 사망 여부

    [Header("Respawn Settings")]
    public float respawnDelay = 8f;     // 리스폰 시간
    public Transform spawnPoint;        // 지정 스폰 포인트

    [Header("Invincible Settings")]
    public float invincibleTime = 0.2f;         // 피격 직후 무적 시간 
    public float respawnInvincibleTime = 2f;    // 리스폰 후 무적 유지 시간
    private bool isInvincible = false;          // 현재 무적 상태 여부

    private AnimationHandler animationHandler;

    private void Start()
    {
        currentHealth = MaxHealth;                                                                     // 게임 시작 시 체력을 최대 체력으로 초기화
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    public void TakeDamage(int damage) // 데미지 함수
    {
        if(isDead || isInvincible) return;                              // 죽으면 데미지 무시

        currentHealth -= damage;                                        // 체력을 데미지 받는 만큼 깍고
        currentHealth = Mathf.Max(currentHealth, 0);                    // 최소값을 0으로 제한

        animationHandler.HitTrigger();

        // 피격 직후 짧은 무적 프레임 적용(중복 피격 방지)
        StartCoroutine(InvincibleCoroutine(invincibleTime));

        if(currentHealth <= 0) // 체력이 0이되면 Die 함수 실행
        {
            Die();
        }
    }
    
    public void Heal(int amount) // 체력을 회복하는 함수
    {
        if(isDead) return;          // 죽으면 힐 불가능
        currentHealth += amount;    // amount 만큼 체력을 회복
        currentHealth = Mathf.Min(currentHealth,MaxHealth); // 최대 체력을 넘지않도록 제한

        Debug.Log($"{characterName} 회복 {amount} -> 체력 : {currentHealth}");  // 로그로 회복 -> 체력 상태 출력
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        isInvincible = true;        // 무적 시간
        yield return new WaitForSeconds(duration);  // duration초 기다림
        isInvincible = false;       // 무적 해제
    }

    void Die()  // 사망 처리 함수
    {
        if(isDead) return;      // 이미 죽어있으면 중복 실행 방지
        isDead = true;

        animationHandler.OnDead();


        // 이동 공격 입력 차단
        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerAttack>().enabled = false;

        // 리스폰 되는 코루틴 실행
        StartCoroutine(Respawn());      
    }

    // 리스폰 처리
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay); // 리스폰 시간 기다린 후 리스폰

        // 위치 초기화
        if (spawnPoint != null)              // spawnPoint가 지정 되어있으면 해당 위치로 이동
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
                cc.enabled = true;
            }
            else
            {
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
            }
        }

        // 체력 리셋
        currentHealth = MaxHealth;
        isDead = false;

        animationHandler.Respawn();

        Debug.Log($"{characterName} 리스폰 완료 → 체력 {currentHealth}");

        // 리스폰 후 2초간 무적
        StartCoroutine(InvincibleCoroutine(respawnInvincibleTime));
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