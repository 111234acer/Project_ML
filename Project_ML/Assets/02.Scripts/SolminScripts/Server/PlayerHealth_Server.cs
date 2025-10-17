using System.Collections;
using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PlayerHealth_Server : MonoBehaviourPun
{
    public int maxHealth = 200;
    public int attackDamage = 20;
    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isDead;

    public float respawnDelay = 8f;
    public Transform spawnPoint;
    public float invincibleTime = 0.2f;
    public float respawnInvincibleTime = 2f;

    private bool isInvincible;
    private CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        currentHealth = maxHealth;
    }

    [PunRPC]
    public void Server_ApplyDamage(int dmg)
    {
        if (!PhotonNetwork.IsMasterClient || isDead || isInvincible) return;
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        if (currentHealth <= 0) Die();
        else StartCoroutine(Invincible(invincibleTime));
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        cc.enabled = false;
        Debug.Log($"{name} 사망!");

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        currentHealth = maxHealth;
        isDead = false;
        cc.enabled = true;
        StartCoroutine(Invincible(respawnInvincibleTime));
        Debug.Log($"{name} 리스폰 완료");
    }

    IEnumerator Invincible(float dur)
    {
        isInvincible = true;
        yield return new WaitForSeconds(dur);
        isInvincible = false;
    }

    public float GetHealthPercent() => (float)currentHealth / maxHealth;
}
