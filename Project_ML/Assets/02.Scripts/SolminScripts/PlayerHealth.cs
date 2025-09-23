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

    private void Start()
    {
        currentHealth = MaxHealth;      // 게임 시작 시 체력을 최대 체력으로 초기화
        Debug.Log($"{characterName} 시작 ! 채력 : {currentHealth}, 공격력 : {attackDamage}");          // 로그로 캐릭터 시작 상태 출력
    }

    public void TakeDamage(int damage) // 데미지 함수
    {
        if(isDead) return;                                              // 죽으면 데미지 무시

        currentHealth -= damage;                                        // 체력을 데미지 받는 만큼 깍고
        currentHealth = Mathf.Max(currentHealth, 0);                    // 최소값을 0으로 제한

        Debug.Log($"{characterName} 피해 {damage} -> 남은 체력 : {currentHealth}");

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

    void Die()  // 사망 처리 함수
    {
        if(isDead) return;      // 이미 죽어있으면 중복 실행 방지
        isDead = true;

        Debug.Log($"{characterName} 사망!");

        // 사망 에니메이션 추가 예정
        StartCoroutine(Respawn());      // 리스폰 되는 코루틴 실행
    }

    // 리스폰 처리
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay); // 리스폰 시간 기다린 후 리스폰

        // 위치 초기화
        if(spawnPoint != null)              // spawnPoint가 지정 되어있으면 해당 위치로 이동
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        // 체력 리셋
        currentHealth = MaxHealth;
        isDead = false;

        Debug.Log($"{characterName} 리스폰 완료 → 체력 {currentHealth}");
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
